export default {
    defaultTheme: "light",
    iconLinks: [
        {
            icon: "github",
            href: "https://github.com/nullmake/calm-dotnet",
            title: "GitHub",
        },
    ],
    start: () => {
        setupPagefind();
    },
};

function setupPagefind() {
    if (document.getElementById("pagefind-modal-container")) {
        return;
    }
    const navContainer = document.querySelector("#navbar");
    if (!navContainer) {
        console.error("#navbar was not found.");
        return;
    }
    if (!document.querySelector('link[href*="pagefind-component-ui.css"]')) {
        const link = document.createElement("link");
        link.href = "pagefind/pagefind-component-ui.css";
        link.rel = "stylesheet";
        document.head.appendChild(link);
    }
    if (!document.querySelector('script[src*="pagefind-component-ui.js"]')) {
        const script = document.createElement("script");
        script.src = "pagefind/pagefind-component-ui.js";
        script.type = "module";
        document.head.appendChild(script);
    }
    const modalContainer = document.createElement("div");
    modalContainer.id = "pagefind-modal-container";
    modalContainer.className = "d-flex align-items-center order-last";
    modalContainer.innerHTML = `
    <pagefind-config bundle-path="/calm-dotnet/pagefind/"></pagefind-config>
    <pagefind-modal-trigger></pagefind-modal-trigger>
    <pagefind-modal></pagefind-modal>
    `;
    navContainer.appendChild(modalContainer);
}
