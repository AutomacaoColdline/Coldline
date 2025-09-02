// wwwroot/js/clipboard.js
window.copyToClipboard = async function (text) {
  try {
    // Tenta usar a API moderna
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(text);
      return true;
    }
    // Fallback (área de transferência via <textarea>)
    const ta = document.createElement("textarea");
    ta.value = text;
    ta.style.position = "fixed";
    ta.style.left = "-9999px";
    document.body.appendChild(ta);
    ta.focus();
    ta.select();
    const ok = document.execCommand("copy");
    document.body.removeChild(ta);
    return ok;
  } catch (e) {
    console.error("copyToClipboard failed", e);
    return false;
  }
};
