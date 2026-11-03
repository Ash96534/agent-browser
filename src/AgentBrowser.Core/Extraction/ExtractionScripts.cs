namespace AgentBrowser.Core.Extraction;

/// <summary>
/// JavaScript snippets used by the extraction service.
/// </summary>
internal static class ExtractionScripts
{
    public const string Metadata = """
    () => ({
      title: document.title || null,
      description: document.querySelector('meta[name="description"]')?.content ?? null,
      canonicalUrl: document.querySelector('link[rel="canonical"]')?.href ?? null,
      openGraph: Object.fromEntries([...document.querySelectorAll('meta[property^="og:"]')]
        .map(m => [m.getAttribute('property'), m.content || ''])
        .filter(([, value]) => value)),
      twitter: Object.fromEntries([...document.querySelectorAll('meta[name^="twitter:"]')]
        .map(m => [m.getAttribute('name'), m.content || ''])
        .filter(([, value]) => value)),
      jsonLd: [...document.querySelectorAll('script[type="application/ld+json"]')]
        .flatMap(script => {
          try {
            const parsed = JSON.parse(script.textContent || '');
            return Array.isArray(parsed) ? parsed : [parsed];
          } catch {
            return [];
          }
        }),
      metaTags: [...document.querySelectorAll('meta')]
        .map(m => ({ name: m.getAttribute('name'), property: m.getAttribute('property'), content: m.content || null }))
    })
    """;

    public const string Prices = """
    () => {
      const contextOf = el => el.closest('[data-context]')?.getAttribute('data-context') ??
          (el.closest('article') ? 'article' : el.closest('[data-product]') ? 'product' : 'page');
      const selectorOf = el => {
        if (el.id) return `#${el.id}`;
        const classes = [...el.classList].join('.');
        return classes ? `.${classes}` : el.tagName.toLowerCase();
      };
      const currencyOf = text => /\$/.test(text) ? ['USD', '$'] :
          /€/.test(text) ? ['EUR', '€'] :
          /£/.test(text) ? ['GBP', '£'] : [null, null];
      const parseValue = text => {
        const match = String(text ?? '').match(/\d+(?:[.,]\d{1,2})?/);
        return match ? Number(match[0].replace(',', '.')) : null;
      };
      const items = [];
      for (const el of document.querySelectorAll('[data-price], [itemprop="price"]')) {
        const raw = el.getAttribute('data-price') ?? el.getAttribute('content') ?? el.textContent ?? '';
        const [currency, symbol] = currencyOf(raw);
        items.push({ value: parseValue(raw), currency, currencySymbol: symbol, context: contextOf(el), selector: selectorOf(el) });
      }
      const existing = new Set(items.map(item => item.selector));
      for (const el of document.querySelectorAll('body *')) {
        if (el.children.length !== 0 || el.closest('script, style')) continue;
        const text = el.textContent?.trim() ?? '';
        if (!/(\$|€|£)\s*\d/.test(text)) continue;
        const selector = selectorOf(el);
        if (existing.has(selector)) continue;
        const [currency, symbol] = currencyOf(text);
        items.push({ value: parseValue(text), currency, currencySymbol: symbol, context: contextOf(el), selector });
      }
      return items;
    }
    """;

    public const string Images = """
    () => [...document.querySelectorAll('img')].map(img => {
      const figure = img.closest('figure');
      const caption = figure?.querySelector('figcaption')?.textContent?.trim() ?? null;
      const parentContext = img.closest('article') ? 'article' :
          img.closest('section') ? 'section' :
          img.closest('[data-product]') ? 'product-card' :
          img.closest('li') ? 'list-item' : 'page';
      return {
        src: img.currentSrc || img.src || img.getAttribute('data-src') || null,
        alt: img.alt || null,
        title: img.title || null,
        width: img.getAttribute('width') ? Number(img.getAttribute('width')) :
            (img.naturalWidth || img.width || null),
        height: img.getAttribute('height') ? Number(img.getAttribute('height')) :
            (img.naturalHeight || img.height || null),
        caption,
        parentContext
      };
    })
    """;

    public const string Forms = """
    () => [...document.querySelectorAll('form')].map(form => ({
      action: form.getAttribute('action') || null,
      method: (form.getAttribute('method') || 'get').toUpperCase(),
      fields: [...form.querySelectorAll('input, textarea, select')].map(field => {
        const labels = [...(field.labels ?? [])].map(label => label.textContent.trim()).filter(Boolean);
        return {
          name: field.getAttribute('name') || null,
          type: (field.getAttribute('type') || field.tagName).toLowerCase(),
          value: field.value ?? null,
          placeholder: field.getAttribute('placeholder') || null,
          required: field.required === true,
          label: labels[0] ?? field.getAttribute('aria-label') ?? null
        };
      })
    }))
    """;

    public const string Links = """
    () => [...document.querySelectorAll('a[href]')]
      .map(anchor => {
        let href = anchor.getAttribute('href');
        try {
          href = new URL(href, document.baseURI).href;
        } catch {
          href = null;
        }
        return {
          href,
          text: anchor.textContent?.trim() || null,
          title: anchor.getAttribute('title') || null,
          rel: anchor.getAttribute('rel') || null,
          target: anchor.getAttribute('target') || null
        };
      })
      .filter(link => link.href)
    """;
}
