export default {
    defaultTheme: 'dark',
    iconLinks: [],
    start: () => {
        setupFrameworkDropdown();
    }
}

function setupFrameworkDropdown() {
    // Check if we are inside an API path
    const path = window.location.pathname;
    if (!path.includes('/api/')) return;

    // Extract current framework from URL (e.g., net10.0, net481, net472)
    const match = /\/api\/([^\/]+)\//.exec(path);
    const currentFw = match ? match[1] : 'net10.0';

    // Create container in the navbar
    const navbar = document.querySelector('.navbar-right') || document.querySelector('.navbar-nav');
    if (!navbar) return;

    const container = document.createElement('div');
    container.className = 'framework-selector nav-item ms-3 d-flex align-items-center';
    container.innerHTML = `
    <label class="me-2 text-secondary small" for="fw-select">Target:</label>
    <select id="fw-select" class="form-select form-select-sm bg-dark text-light border-secondary">
      <option value="net472" ${currentFw === 'net472' ? 'selected' : ''}>.NET Framework 4.7.2</option>
      <option value="net481" ${currentFw === 'net481' ? 'selected' : ''}>.NET Framework 4.8.1</option>
      <option value="net10.0" ${currentFw === 'net10.0' ? 'selected' : ''}>.NET 10.0</option>
    </select>
  `;

    navbar.prepend(container);

    // Handle switching logic
    document.getElementById('fw-select').addEventListener('change', (e) => {
        const selectedFw = e.target.value;
        // Swap the framework segment in the current URL safely
        const newPath = path.replace(/\/api\/[^\/]+\//, `/api/${selectedFw}/`);

        // Test if the page exists or fallback to the root of that framework API
        window.location.href = newPath;
    });
}