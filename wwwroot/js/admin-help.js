(function(){
  function openAdminHelp(){
    var overlay = document.getElementById('adminHelpOverlay');
    if(!overlay) return;
    overlay.classList.add('show');
  }

  function closeAdminHelp(){
    var overlay = document.getElementById('adminHelpOverlay');
    if(!overlay) return;
    overlay.classList.remove('show');
  }

  window.openAdminHelp = openAdminHelp;
  window.closeAdminHelp = closeAdminHelp;

  // F12 helper
  document.addEventListener('keydown', function(e){
    if(e.key === 'F12'){
      // Prevent default? let browser handle, but show hints.
      openAdminHelp();
    }
  });

  // Buttons
  document.addEventListener('click', function(e){
    var btn = e.target && e.target.closest && e.target.closest('[data-open-admin-help]');
    if(btn){
      e.preventDefault();
      openAdminHelp();
    }
  });
})();

