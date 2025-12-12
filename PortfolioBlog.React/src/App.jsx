import { useEffect, useState } from "react";
import {
    getPublicArticles,
    getMyArticles,
    login,
    clearToken,
    getToken,
    createArticle,
    register,
} from "./api";

function Card({ title, children }) {
    return (
        <div
            style={{
                background: "#fff",
                border: "1px solid #e6e6e6",
                borderRadius: 16,
                padding: 18,
                boxShadow: "0 6px 20px rgba(0,0,0,0.06)",
            }}
        >
            <div style={{ fontSize: 14, opacity: 0.7, marginBottom: 6 }}>{title}</div>
            {children}
        </div>
    );
}

function Input(props) {
    return (
        <input
            {...props}
            style={{
                width: "100%",
                padding: "10px 12px",
                borderRadius: 12,
                border: "1px solid #ddd",
                outline: "none",
                fontSize: 14,
                ...props.style,
            }}
        />
    );
}

function TextArea(props) {
    return (
        <textarea
            {...props}
            style={{
                width: "100%",
                padding: "10px 12px",
                borderRadius: 12,
                border: "1px solid #ddd",
                outline: "none",
                fontSize: 14,
                resize: "vertical",
                ...props.style,
            }}
        />
    );
}

function Button({ variant = "primary", ...props }) {
    const bg = variant === "primary" ? "#111" : variant === "danger" ? "#b91c1c" : "#fff";
    const color = variant === "ghost" ? "#111" : "#fff";
    const border = variant === "ghost" ? "1px solid #ddd" : "1px solid transparent";

    return (
        <button
            {...props}
            style={{
                padding: "10px 12px",
                borderRadius: 12,
                border,
                background: variant === "ghost" ? "#fff" : bg,
                color: variant === "ghost" ? "#111" : color,
                fontWeight: 600,
                cursor: "pointer",
                width: "100%",
            }}
        />
    );
}

export default function App() {
    const [publicArticles, setPublicArticles] = useState([]);
    const [myArticles, setMyArticles] = useState([]);

    const [auth, setAuth] = useState(!!getToken());

    // login form
    const [email, setEmail] = useState("author@demo.com");
    const [password, setPassword] = useState("Author123!");

    // register form
    const [regEmail, setRegEmail] = useState("");
    const [regPassword, setRegPassword] = useState("");

    // create article form
    const [title, setTitle] = useState("");
    const [content, setContent] = useState("");
    const [isPublished, setIsPublished] = useState(true);

    const [loading, setLoading] = useState(false);
    const [msg, setMsg] = useState("");

    async function reloadPublic() {
        const data = await getPublicArticles();
        setPublicArticles(data);
    }

    async function reloadMine() {
        if (!getToken()) return;
        const data = await getMyArticles();
        setMyArticles(data);
    }

    useEffect(() => {
        reloadPublic();
        if (auth) reloadMine();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    async function onLogin(e) {
        e.preventDefault();
        setLoading(true);
        setMsg("");
        try {
            await login(email, password);
            setAuth(true);
            setMsg("Connecté ✅");
            await reloadMine();
        } catch (err) {
            setMsg("Login ❌");
            alert("Login ❌ " + err.message);
            console.error(err);
        } finally {
            setLoading(false);
        }
    }

    async function onRegister(e) {
        e.preventDefault();
        setLoading(true);
        setMsg("");
        try {
            await register(regEmail, regPassword);
            setMsg("Compte créé ✅ (tu peux te connecter)");
            setRegEmail("");
            setRegPassword("");
        } catch (err) {
            setMsg("Register ❌");
            // tu verras exactement la raison (password, email déjà pris, etc.)
            alert("Register ❌ " + err.message);
            console.error(err);
        } finally {
            setLoading(false);
        }
    }

    function onLogout() {
        clearToken();
        setAuth(false);
        setMyArticles([]);
        setMsg("Déconnecté ✅");
    }

    async function onCreateArticle(e) {
        e.preventDefault();
        setLoading(true);
        setMsg("");
        try {
            const a = await createArticle(title, content, isPublished);
            setMsg(`Article créé ✅ : ${a.title}`);
            setTitle("");
            setContent("");
            await reloadPublic();
            await reloadMine();
        } catch (err) {
            setMsg("Création ❌");
            alert("Création ❌ " + err.message);
            console.error(err);
        } finally {
            setLoading(false);
        }
    }

    return (
        <div
            style={{
                minHeight: "100vh",
                background:
                    "radial-gradient(900px 500px at 20% 10%, rgba(0,0,0,0.08), transparent), radial-gradient(700px 400px at 90% 20%, rgba(0,0,0,0.06), transparent), #f6f6f6",
                padding: 24,
            }}
        >
            <div style={{ maxWidth: 980, margin: "0 auto" }}>
                <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", gap: 16, marginBottom: 18 }}>
                    <div>
                        <h1 style={{ margin: 0, fontSize: 28 }}>Portfolio Blog</h1>
                        <div style={{ opacity: 0.7, marginTop: 4, fontSize: 14 }}>
                            API: <code>{import.meta.env.VITE_API_URL}</code>
                        </div>
                    </div>

                    <div style={{ width: 220 }}>
                        {auth ? (
                            <Button variant="danger" onClick={onLogout}>
                                Logout
                            </Button>
                        ) : (
                            <Button variant="ghost" onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}>
                                Aller au login
                            </Button>
                        )}
                    </div>
                </div>

                {msg && (
                    <div
                        style={{
                            marginBottom: 16,
                            padding: "10px 12px",
                            borderRadius: 12,
                            background: "#fff",
                            border: "1px solid #e6e6e6",
                            fontSize: 14,
                        }}
                    >
                        {msg}
                    </div>
                )}

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16, marginBottom: 18 }}>
                    {!auth ? (
                        <>
                            <Card title="Connexion">
                                <form onSubmit={onLogin}>
                                    <div style={{ display: "grid", gap: 10 }}>
                                        <Input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="email" />
                                        <Input value={password} onChange={(e) => setPassword(e.target.value)} placeholder="password" type="password" />
                                        <Button disabled={loading} type="submit">
                                            {loading ? "..." : "Se connecter"}
                                        </Button>
                                    </div>
                                </form>
                            </Card>

                            <Card title="Créer un compte (public)">
                                <form onSubmit={onRegister}>
                                    <div style={{ display: "grid", gap: 10 }}>
                                        <Input value={regEmail} onChange={(e) => setRegEmail(e.target.value)} placeholder="email" />
                                        <Input value={regPassword} onChange={(e) => setRegPassword(e.target.value)} placeholder="password" type="password" />
                                        <div style={{ fontSize: 12, opacity: 0.7 }}>
                                            Le rôle est forcé à <b>Author</b> côté API.
                                        </div>
                                        <Button disabled={loading} type="submit" variant="ghost">
                                            {loading ? "..." : "Register"}
                                        </Button>
                                    </div>
                                </form>
                            </Card>
                        </>
                    ) : (
                        <Card title="Créer un article">
                            <form onSubmit={onCreateArticle}>
                                <div style={{ display: "grid", gap: 10 }}>
                                    <Input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Titre" />
                                    <TextArea value={content} onChange={(e) => setContent(e.target.value)} placeholder="Contenu" rows={6} />
                                    <label style={{ display: "flex", gap: 8, alignItems: "center", fontSize: 14 }}>
                                        <input type="checkbox" checked={isPublished} onChange={(e) => setIsPublished(e.target.checked)} />
                                        Publié
                                    </label>
                                    <Button disabled={loading} type="submit">
                                        {loading ? "..." : "Créer"}
                                    </Button>
                                </div>
                            </form>
                        </Card>
                    )}

                    <Card title="Infos">
                        <div style={{ fontSize: 14, lineHeight: 1.5 }}>
                            <div>
                                Statut: <b style={{ color: auth ? "#16a34a" : "#b91c1c" }}>{auth ? "Connecté" : "Non connecté"}</b>
                            </div>
                            <div style={{ marginTop: 8, opacity: 0.75 }}>
                                - Public: <code>/articles/public</code> <br />
                                - Mes articles (draft inclus): <code>/articles</code>
                            </div>
                        </div>
                    </Card>
                </div>

                {auth && (
                    <div style={{ marginBottom: 18 }}>
                        <Card title="Mes articles (drafts inclus)">
                            {myArticles.length === 0 ? (
                                <div style={{ opacity: 0.7 }}>Aucun article pour le moment.</div>
                            ) : (
                                <div style={{ display: "grid", gap: 12 }}>
                                    {myArticles.map((a) => (
                                        <div
                                            key={a.id}
                                            style={{
                                                padding: 14,
                                                border: "1px solid #eee",
                                                borderRadius: 14,
                                                background: "linear-gradient(180deg, #fff, #fafafa)",
                                            }}
                                        >
                                            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 12 }}>
                                                <h3 style={{ margin: 0, fontSize: 18 }}>{a.title}</h3>
                                                <span
                                                    style={{
                                                        padding: "4px 10px",
                                                        borderRadius: 999,
                                                        fontSize: 12,
                                                        background: a.isPublished ? "#1f6f3b" : "#6b2b2b",
                                                        color: "white",
                                                        fontWeight: 700,
                                                    }}
                                                >
                                                    {a.isPublished ? "Publié" : "Brouillon"}
                                                </span>
                                            </div>
                                            <small style={{ opacity: 0.75 }}>{new Date(a.createdAtUtc).toLocaleString()}</small>
                                            {a.slug && <div style={{ marginTop: 6, fontSize: 12, opacity: 0.7 }}>slug: {a.slug}</div>}
                                        </div>
                                    ))}
                                </div>
                            )}
                        </Card>
                    </div>
                )}

                <Card title="Articles publiés">
                    {publicArticles.length === 0 ? (
                        <div style={{ opacity: 0.7 }}>Aucun article publié pour le moment.</div>
                    ) : (
                        <div style={{ display: "grid", gap: 12 }}>
                            {publicArticles.map((a) => (
                                <div
                                    key={a.id}
                                    style={{
                                        padding: 14,
                                        border: "1px solid #eee",
                                        borderRadius: 14,
                                        background: "linear-gradient(180deg, #fff, #fafafa)",
                                    }}
                                >
                                    <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "baseline" }}>
                                        <h3 style={{ margin: 0, fontSize: 18 }}>{a.title}</h3>
                                        <small style={{ opacity: 0.7 }}>{new Date(a.createdAtUtc).toLocaleString()}</small>
                                    </div>
                                    {a.slug && <div style={{ marginTop: 6, fontSize: 12, opacity: 0.7 }}>slug: {a.slug}</div>}
                                </div>
                            ))}
                        </div>
                    )}
                </Card>
            </div>
        </div>
    );
}
