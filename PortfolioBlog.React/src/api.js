const API_URL = import.meta.env.VITE_API_URL;

export function getToken() {
    return localStorage.getItem("token");
}

export function setToken(token) {
    localStorage.setItem("token", token);
}

export function clearToken() {
    localStorage.removeItem("token");
}

function authHeaders() {
    const token = getToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
}

async function readError(res) {
    // essaie JSON puis fallback texte
    const ct = res.headers.get("content-type") || "";
    if (ct.includes("application/json")) {
        const j = await res.json().catch(() => null);

        // Identity: tableau [{code, description}, ...]
        if (Array.isArray(j)) {
            return j.map((e) => e.description || e.code).join(" | ");
        }

        // string/json classique
        if (typeof j === "string") return j;

        // {message:...} etc
        if (j && typeof j === "object") return JSON.stringify(j);

        return "Erreur JSON inconnue";
    }

    return await res.text().catch(() => `HTTP ${res.status}`);
}

async function request(path, { method = "GET", body, auth = false } = {}) {
    const headers = { "Content-Type": "application/json" };
    if (auth) Object.assign(headers, authHeaders());

    const res = await fetch(`${API_URL}${path}`, {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined,
    });

    if (!res.ok) {
        const err = await readError(res);
        throw new Error(err || `HTTP ${res.status}`);
    }

    const ct = res.headers.get("content-type") || "";
    return ct.includes("application/json") ? res.json() : res.text();
}

// PUBLIC
export function getPublicArticles() {
    return request("/articles/public");
}

// PRIVÉ (token)
export function getMyArticles() {
    return request("/articles", { auth: true });
}

export async function login(email, password) {
    const data = await request("/auth/login", {
        method: "POST",
        body: { email, password },
    });
    setToken(data.access_token);
    return data;
}

// ✅ REGISTER PUBLIC : ton API ignore le rôle et met Author
export function register(email, password) {
    return request("/auth/register", {
        method: "POST",
        body: { email, password }, // pas de role
    });
}

export function createArticle(title, content, isPublished) {
    return request("/articles", {
        method: "POST",
        auth: true,
        body: { title, content, isPublished },
    });
}
