/*!
 * cirreum-loader.js
 * Cirreum Blazor Loader Script
 * =====================================================================
 * Core bootstrap script for Cirreum-based Blazor WebAssembly apps.
 *
 * Responsibilities:
 *  • Resolve and load fingerprinted CSS/JS assets from the Import Map
 *  • Apply Subresource Integrity (SRI) where available
 *  • Initialize the selected Bootstrap color scheme (cirreum-bootstrap-*.css)
 *  • Persist the selected color scheme for future browser sessions
 *  • Load custom fonts, vendor libraries, and application resources
 *  • Handle authentication library loading (MSAL/OIDC)
 *  • Register PWA service workers when configured
 *  • Bootstrap the Blazor Error UI with Cirreum styling
 *  • Provide helper utilities for diagnostics and fallback loading
 *
 * Notes:
 *  • Script attributes on the <script> tag control behavior:
 *      - asm-name     : Client assembly name  
 *      - app-name     : Friendly public name  
 *      - app-theme    : Bootstrap color scheme  
 *                      (default | aqua | aspire | excel | office | outlook | windows)
 *      - font-name    : Custom font name (from cdnfonts) or "default"
 *      - auth-type    : Authentication mode  
 *                      (msal | oidc | none)
 *      - pwa-script   : Service worker file for PWA scenarios
 *
 * Version:     @VERSION@
 * License:     MIT
 * Repository:  https://github.com/cirreum/Cirreum.Components.WebAssembly
 * Copyright:   2025 Cirreum Contributors
 * =====================================================================
 */


// Track if we successfully loaded a persisted scheme
let themeFromStorage = false;
const validThemes = ["default", "aqua", "aspire", "excel", "office", "outlook", "windows", "none", "custom"];

const asmName = document.currentScript.getAttribute("asm-name");
const appName = document.currentScript.getAttribute("app-name");
let appTheme = document.currentScript.getAttribute("app-theme")?.trim() || "default";
const fontName = document.currentScript.getAttribute("font-name");
const authType = document.currentScript.getAttribute("auth-type");
const pwaScript = document.currentScript.getAttribute("pwa-script");

if (authType) {
	window.authConfig = { include: true, authType: authType.toLowerCase() };
} else {
	window.authConfig = { include: false, authType: "" };
}

// Validate and fallback to default if invalid
if (!validThemes.includes(appTheme)) {
	console.warn(`Invalid app-theme "${appTheme}". Falling back to "default".`);
	appTheme = "default";
}


window.loadCss = (href, integrity, title, disabled) => {
	const linkCss = document.createElement("link");
	linkCss.crossOrigin = "anonymous";
	linkCss.referrerPolicy = "no-referrer";
	linkCss.rel = "stylesheet";
	linkCss.type = "text/css";

	// Resolve fingerprinted URL
	const resolvedHref = resolveFingerprintedUrl(href);

	// If no explicit integrity provided, try import map
	if (!integrity) {
		integrity = resolveIntegrityFor(resolvedHref);
	}

	if (integrity) linkCss.integrity = integrity;
	if (title) linkCss.title = title;
	if (disabled) linkCss.disabled = disabled;

	linkCss.onerror = () => console.error('Error occurred while loading stylesheet', resolvedHref);
	linkCss.href = resolvedHref;
	document.head.appendChild(linkCss);
};

window.loadJs = (src, integrity) => {
	const scriptJs = document.createElement("script");
	scriptJs.crossOrigin = "anonymous";
	scriptJs.referrerPolicy = "no-referrer";
	scriptJs.async = false;
	scriptJs.defer = true;

	// Resolve fingerprinted URL
	const resolvedSrc = resolveFingerprintedUrl(src);

	// If integrity wasn’t passed, check import map
	if (!integrity) {
		integrity = resolveIntegrityFor(resolvedSrc);
	}

	if (integrity) scriptJs.integrity = integrity;
	scriptJs.src = resolvedSrc;

	scriptJs.onerror = () => console.error('Error occurred while loading script', resolvedSrc);
	document.body.appendChild(scriptJs);
};


function loadLocalCss(href) {
	const linkCss = document.createElement("link");
	linkCss.rel = "stylesheet";
	linkCss.type = "text/css";

	const resolvedHref = resolveFingerprintedUrl(href);
	const integrity = resolveIntegrityFor(resolvedHref);

	if (integrity) {
		linkCss.crossOrigin = "anonymous";
		linkCss.referrerPolicy = "no-referrer";
		linkCss.integrity = integrity;
	}

	linkCss.onerror = () => {
		console.error('Error occurred while loading stylesheet', resolvedHref);
		if (resolvedHref !== href) {
			console.log('Attempting fallback to non-fingerprinted stylesheet:', href);
			const fallbackLink = document.createElement("link");
			fallbackLink.rel = "stylesheet";
			fallbackLink.type = "text/css";
			fallbackLink.href = href;
			document.head.appendChild(fallbackLink);
		}
	};

	linkCss.href = resolvedHref;
	document.head.appendChild(linkCss);
}
function loadLocalJs(src) {
	const scriptJs = document.createElement("script");

	const resolvedSrc = resolveFingerprintedUrl(src);
	const integrity = resolveIntegrityFor(resolvedSrc);

	if (integrity) {
		scriptJs.crossOrigin = "anonymous";
		scriptJs.referrerPolicy = "no-referrer";
		scriptJs.integrity = integrity;
	}

	scriptJs.src = resolvedSrc;
	scriptJs.onerror = () => {
		console.error('Error occurred while loading script', resolvedSrc);
		if (resolvedSrc !== src) {
			console.log('Attempting fallback to non-fingerprinted version:', src);
			const fallbackScript = document.createElement("script");
			fallbackScript.src = src;
			fallbackScript.onerror = () => console.error('Fallback also failed for:', src);
			document.body.appendChild(fallbackScript);
		}
	};
	document.body.appendChild(scriptJs);
}

function resolveIntegrityFor(url) {
	const importMapScript = document.querySelector('script[type="importmap"]');
	if (!importMapScript) return null;

	try {
		const importMap = JSON.parse(importMapScript.textContent || importMapScript.innerHTML);
		if (importMap.integrity) {
			// Try with ./ prefix first (common in Blazor importmaps)
			let lookupPath = './' + url;
			if (importMap.integrity[lookupPath]) {
				return importMap.integrity[lookupPath];
			}
			if (importMap.integrity[url]) {
				return importMap.integrity[url];
			}
		}
	} catch (e) {
		console.warn('Failed to parse import map integrity for', url, e);
	}
	return null;
}

function resolveFingerprintedUrl(originalPath) {
	const importMapScript = document.querySelector('script[type="importmap"]');
	if (importMapScript) {
		try {
			const importMap = JSON.parse(importMapScript.textContent || importMapScript.innerHTML);

			// Try with ./ prefix first (as shown in import map)
			let lookupPath = './' + originalPath;
			if (importMap.imports && importMap.imports[lookupPath]) {
				return importMap.imports[lookupPath].substring(2); // Remove './'
			}

			// Try without prefix
			if (importMap.imports && importMap.imports[originalPath]) {
				return importMap.imports[originalPath];
			}
		} catch (e) {
			console.warn('Failed to resolve fingerprinted URL for:', originalPath, e);
		}
	}
	return originalPath;
}

function buildBlazorErrorUI() {

	const createElementWithProps = (tag, props = {}) => {
		const element = document.createElement(tag);
		Object.assign(element, props);
		return element;
	};

	const icon = createElementWithProps("i", {
		className: "bi bi-exclamation-triangle-fill me-2",
		role: "presentation",
		"aria-hidden": "true"
	});

	const messageSpan = createElementWithProps("span", {
		innerHTML: "An unhandled error has occurred."
	});

	const reloadLink = createElementWithProps("a", {
		href: "#",
		className: "reload ms-3",
		innerHTML: "Reload"
	});

	const dismissButton = createElementWithProps("button", {
		id: "blazor-error-ui-dismiss",
		type: "button",
		className: "btn-close",
		"aria-label": "Dismiss error message"
	});
	dismissButton.setAttribute("data-bs-dismiss", "alert");

	const errorUI = createElementWithProps("div", {
		id: "blazor-error-ui",
		className: "fs-6 alert alert-warning alert-dismissible fade show my-1 mx-3 fixed-bottom align-items-center",
		role: "alert",
		style: "display:none; z-index:10001;"
	});

	[icon, messageSpan, reloadLink, dismissButton].forEach(el => errorUI.appendChild(el));
	document.body.appendChild(errorUI);

	const config = { attributes: true, childList: false, subtree: false };
	const observer = new MutationObserver((mutations) => {
		for (const mutation of mutations) {
			if (mutation.type === 'attributes' && mutation.attributeName === 'style') {
				// disconnect before we modify the node
				observer.disconnect();
				mutation.target.style.display = "flex";
				// re-connect after we modify the node
				observer.observe(errorUI, config);
			}
		}
	});

	dismissButton.addEventListener("click", () => {
		// disconnect before we modify the node
		observer.disconnect();
		errorUI.style.display = "none";
		// re-connect after we modify the node
		observer.observe(errorUI, config);
	});

	observer.observe(errorUI, config);
}

function initializeApp() {

	//
	// Html */
	//
	buildBlazorErrorUI();


	//
	// Custom Font */
	//
	if (fontName && fontName.toLowerCase() !== "default") {
		window.loadCss(`https://fonts.cdnfonts.com/css/${fontName}`);
		document.documentElement.style.setProperty("--bs-font-sans-serif", `\"${fontName}\", sans-serif`);
	}


	//
	// Stylesheets */
	//

	// cirreum-bootstrap-{scheme.Id}.css
	//
	// Allow persisted scheme to override the script attribute
	// (so a user choice survives reloads)
	try {
		const storedScheme = localStorage.getItem("user-color-scheme");
		if (storedScheme && validThemes.includes(storedScheme)) {
			appTheme = storedScheme;
			themeFromStorage = true;
		}
	} catch {
		// Ignore storage errors (private mode, etc.)
	}
	// Load appropriate Bootstrap CSS
	if (appTheme !== "none" && appTheme !== "custom") {

		// Only persist if this value did *not* come from storage already
		if (!themeFromStorage) {
			try {
				localStorage.setItem("user-color-scheme", appTheme);
			} catch {
				// ignore if storage is unavailable
			}
		}

		document.documentElement.setAttribute("data-color-scheme", appTheme);
		loadLocalCss(`_content/Cirreum.Components.WebAssembly/css/cirreum-bootstrap-${appTheme}.css`);
	} else {
		document.documentElement.removeAttribute("data-color-scheme");
		console.info(
			"Custom theme mode: No Bootstrap CSS loaded. " +
			"Please provide your own compiled Bootstrap CSS that includes Cirreum customizations."
		);
	}

	// cirreum-spinners.css
	loadLocalCss("_content/Cirreum.Components.WebAssembly/css/cirreum-spinners.css");

	// bootstrap-icons.min.css
	// Lastest: v1.13.1
	// https://unpkg.com/bootstrap-icons@1.13.1/?meta
	loadLocalCss("_content/Cirreum.Components.WebAssembly/css/bootstrap-icons.min.css");


	//
	// JavaScript */
	//

	// auth library...
	if (window.authConfig && window.authConfig.include === true) {
		if (window.authConfig.authType === "msal") {
			loadLocalJs("_content/Microsoft.Authentication.WebAssembly.Msal/AuthenticationService.js");
		} else if (window.authConfig.authType === "oidc") {
			loadLocalJs("_content/Microsoft.AspNetCore.Components.WebAssembly.Authentication/AuthenticationService.js");
		} else {
			window.authConfig.include = false;
		}
	}

	// blazor.webassembly.js
	loadLocalJs("_framework/blazor.webassembly.js");

	// pwa scripts
	if ('serviceWorker' in navigator && pwaScript && pwaScript !== 'none' && pwaScript.endsWith('.js')) {
		navigator.serviceWorker.register(pwaScript);
	}

	// pace.js
	window.paceOptions = {
		ajax: {
			trackMethods: ['GET', 'POST', 'PUT', 'PATCH'],
			ignoreURLs: ['browserLinkSignalR', 'microsoftonline.com', 'applicationinsights.azure.com', 'graph.microsoft.com']
		},
		eventLag: true,
		restartOnPushState: false
	};
	loadLocalJs("_content/Cirreum.Components.WebAssembly/js/pace.min.js");

	// popper.js
	// Latest: v2.11.8
	// https://app.unpkg.com/@popperjs/core@2.11.8
	loadLocalJs("_content/Cirreum.Components.WebAssembly/js/popper.min.js");

	// draggabilly.js
	// Latest: v3.0.0
	// https://app.unpkg.com/draggabilly@3.0.0
	loadLocalJs("_content/Cirreum.Components.WebAssembly/js/draggabilly.pkgd.min.js");

	//
	// App Styles
	//
	if (asmName) {
		window.addEventListener("load", () => {
			loadLocalCss(`${asmName}.styles.css`);
		});
	}

	//
	// App Name
	//
	window.appName = () =>
		appName ||
		window.location.hostname.toUpperCase();

}

if (document.readyState === "loading") {
	document.addEventListener("DOMContentLoaded", initializeApp);
} else {
	initializeApp();
}