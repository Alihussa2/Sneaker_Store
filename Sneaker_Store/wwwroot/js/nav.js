// Faelles navbar-logik. Kaldes paa hver side for at vise/skjule links ud fra login-status.
async function initNav() {
    let bruger = null;
    try {
        const response = await fetch('/api/auth/me');
        if (response.ok) {
            bruger = await response.json();
        }
    } catch {
        bruger = null;
    }

    document.querySelectorAll('[data-nav="min-side"]').forEach(el => el.style.display = bruger ? '' : 'none');
    document.querySelectorAll('[data-nav="admin"]').forEach(el => el.style.display = bruger?.isAdmin ? '' : 'none');
    document.querySelectorAll('[data-nav="logout"]').forEach(el => el.style.display = bruger ? '' : 'none');
    document.querySelectorAll('[data-nav="login"]').forEach(el => el.style.display = bruger ? 'none' : '');

    const logoutLink = document.getElementById('logoutLink');
    if (logoutLink) {
        logoutLink.addEventListener('click', async (e) => {
            e.preventDefault();
            await fetch('/api/auth/logout', { method: 'POST' });
            window.location.href = '/';
        });
    }

    return bruger;
}
