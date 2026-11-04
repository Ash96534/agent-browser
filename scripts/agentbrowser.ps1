[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('health', 'session-create', 'session-close', 'navigate', 'html', 'content', 'links', 'screenshot')]
    [string]$Command,

    [string]$BaseUrl = 'http://localhost:5000',
    [string]$SessionId,
    [string]$Url,
    [string]$Output = 'screenshot.png'
)

$ErrorActionPreference = 'Stop'

switch ($Command) {
    'health' {
        Invoke-RestMethod -Uri "$BaseUrl/health" | ConvertTo-Json -Depth 10
    }

    'session-create' {
        $session = Invoke-RestMethod -Method Post -Uri "$BaseUrl/session" `
            -ContentType 'application/json' -Body '{"headless":true}'
        $session | ConvertTo-Json -Depth 10
    }

    'session-close' {
        if (-not $SessionId) { throw '-SessionId is required' }
        Invoke-RestMethod -Method Delete -Uri "$BaseUrl/session/$SessionId" | ConvertTo-Json -Depth 10
    }

    'navigate' {
        if (-not $SessionId -or -not $Url) { throw '-SessionId and -Url are required' }
        Invoke-RestMethod -Method Post -Uri "$BaseUrl/session/$SessionId/navigate" `
            -ContentType 'application/json' -Body (@{ url = $Url } | ConvertTo-Json) | ConvertTo-Json -Depth 10
    }

    'html' {
        if (-not $SessionId) { throw '-SessionId is required' }
        (Invoke-RestMethod -Uri "$BaseUrl/session/$SessionId/html").html
    }

    'content' {
        if (-not $SessionId) { throw '-SessionId is required' }
        (Invoke-RestMethod -Uri "$BaseUrl/session/$SessionId/content?format=text").html
    }

    'links' {
        if (-not $SessionId) { throw '-SessionId is required' }
        Invoke-RestMethod -Uri "$BaseUrl/session/$SessionId/extract/links" | ConvertTo-Json -Depth 10
    }

    'screenshot' {
        if (-not $SessionId) { throw '-SessionId is required' }
        $result = Invoke-RestMethod -Uri "$BaseUrl/session/$SessionId/screenshot/base64"
        [IO.File]::WriteAllBytes($Output, [Convert]::FromBase64String($result.base64))
        Write-Output "Wrote $Output"
    }
}
