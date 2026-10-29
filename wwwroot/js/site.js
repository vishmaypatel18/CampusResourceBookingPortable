document.addEventListener("DOMContentLoaded", () => {
  // Native HTML validation remains enabled; server-side validation is authoritative.
  document.querySelectorAll("form[ data-confirm]").forEach(form => {
    form.addEventListener("submit", event => {
      if (!confirm(form.dataset.confirm)) event.preventDefault();
    });
  });
});
