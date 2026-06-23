/* TinyMCE init (включён только если на странице есть textarea[data-tinymce="true"]) */

(function () {
  if (typeof window === 'undefined') return;

  function waitForTinyMce(tries) {
    return new Promise((resolve) => {
      let i = 0;
      const t = setInterval(() => {
        i++;
        if (window.tinymce) {
          clearInterval(t);
          resolve(true);
          return;
        }
        if (i >= (tries || 25)) {
          clearInterval(t);
          resolve(false);
        }
      }, 50);
    });
  }

  function mountEditor(el) {
    if (!el || !(el instanceof Element)) return;
    if (el.getAttribute('data-mce-initialized') === 'true') return;
    el.setAttribute('data-mce-initialized', 'true');

    window.tinymce.init({
      target: el,
      language: 'ru',
      height: 420,
      menubar: true,
      branding: false,
      skin: false,
      content_css: false,
      plugins: [
        'advlist',
        'autolink',
        'lists',
        'link',
        'image',
        'charmap',
        'preview',
        'anchor',
        'searchreplace',
        'visualblocks',
        'code',
        'fullscreen',
        'insertdatetime',
        'table',
        'help',
        'wordcount',
      ],
      toolbar:
        'undo redo | blocks | bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | ' +
        'bullist numlist outdent indent | removeformat | link image | table | code | fullscreen | preview',
      block_formats: 'Paragraph=p; Заголовок 3=h3; Заголовок 4=h4; Блок кода=pre',
      image_caption: true,
      automatic_uploads: false,
      convert_urls: false,
      relative_urls: false,
      entity_encoding: 'raw',
      statusbar: true,
      content_style:
        'body { font-family: "Segoe UI", Arial, sans-serif; font-size: 14px; line-height: 1.7; color:#0b1020; } ' +
        'h3 { font-size: 20px; margin: 18px 0 10px; } ' +
        'h4 { font-size: 16px; margin: 14px 0 8px; } ' +
        'pre { background:#0b1020; color:#eaf2ff; padding:12px; border-radius:10px; } ' +
        'blockquote { border-left:4px solid #7c3aed; padding-left:12px; margin: 12px 0; }',
      setup: function (editor) {
        editor.on('init', function () {
          try {
            editor.getContainer().style.border = '1px solid rgba(124,58,237,.35)';
            editor.getContainer().style.borderRadius = '12px';
            editor.getContainer().style.overflow = 'hidden';
          } catch (e) {}
        });
      },
    });
  }

  async function init() {
    const ok = await waitForTinyMce(25);
    if (!ok) {
      console.warn('[TinyMCE] tinymce не загрузился');
      return;
    }

    function mountAll() {
      const els = document.querySelectorAll('textarea[data-tinymce="true"]');
      console.log('[TinyMCE] found:', els.length);
      els.forEach(mountEditor);
    }

    document.addEventListener('DOMContentLoaded', function () {
      mountAll();
      requestAnimationFrame(mountAll);
      setTimeout(mountAll, 250);
    });
  }

  init();
})();


