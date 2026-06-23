(function () {
  const resourceApiBase = '/api/ResourceApi';
  const projectApiBase = '/api/ProjectApi';

  function getCsrfToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
  }

  async function fetchJson(url, options) {
    const res = await fetch(url, options);
    if (!res.ok) {
      let msg = 'Ошибка запроса';
      try {
        const data = await res.json();
        msg = data?.message || msg;
      } catch (_) {}
      throw new Error(msg);
    }
    return await res.json();
  }

  async function getByType(type) {
    return await fetchJson(`${resourceApiBase}/by-type?type=${type}`);
  }

  async function getProjectSummary(projectId) {
    return await fetchJson(`${projectApiBase}/${projectId}/summary`);
  }

  async function updateMargin(projectId, resourceId, marginPercent) {
    const csrf = getCsrfToken();

    return await fetchJson(`${projectApiBase}/update-margin`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': csrf
      },
      body: JSON.stringify({ projectId, resourceId, marginPercent })
    });
  }

  // Expose
  window.resourceApi = {
    getByType,
    updateMargin,
    getProjectSummary
  };
})();

