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
 *                      (default | aspire | excel | office | outlook | windows)
 *      - app-diag     : Diagnostics mode  
 *                      (none | appInsights)
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
const validThemes = ["default", "aspire", "excel", "office", "outlook", "windows", "none", "custom"];

const asmName = document.currentScript.getAttribute("asm-name");
const appName = document.currentScript.getAttribute("app-name");
let appTheme = document.currentScript.getAttribute("app-theme")?.trim() || "default";
const appDiag = document.currentScript.getAttribute("app-diag");
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

function loadAppInsights() {
	!(function (cfg) { function e() { cfg.onInit && cfg.onInit(n) } var x, w, D, t, E, n, C = window, O = document, b = C.location, q = "script", I = "ingestionendpoint", L = "disableExceptionTracking", j = "ai.device."; "instrumentationKey"[x = "toLowerCase"](), w = "crossOrigin", D = "POST", t = "appInsightsSDK", E = cfg.name || "appInsights", (cfg.name || C[t]) && (C[t] = E), n = C[E] || function (g) { var f = !1, m = !1, h = { initialize: !0, queue: [], sv: "8", version: 2, config: g }; function v(e, t) { var n = {}, i = "Browser"; function a(e) { e = "" + e; return 1 === e.length ? "0" + e : e } return n[j + "id"] = i[x](), n[j + "type"] = i, n["ai.operation.name"] = b && b.pathname || "_unknown_", n["ai.internal.sdkVersion"] = "javascript:snippet_" + (h.sv || h.version), { time: (i = new Date).getUTCFullYear() + "-" + a(1 + i.getUTCMonth()) + "-" + a(i.getUTCDate()) + "T" + a(i.getUTCHours()) + ":" + a(i.getUTCMinutes()) + ":" + a(i.getUTCSeconds()) + "." + (i.getUTCMilliseconds() / 1e3).toFixed(3).slice(2, 5) + "Z", iKey: e, name: "Microsoft.ApplicationInsights." + e.replace(/-/g, "") + "." + t, sampleRate: 100, tags: n, data: { baseData: { ver: 2 } }, ver: undefined, seq: "1", aiDataContract: undefined } } var n, i, t, a, y = -1, T = 0, S = ["js.monitor.azure.com", "js.cdn.applicationinsights.io", "js.cdn.monitor.azure.com", "js0.cdn.applicationinsights.io", "js0.cdn.monitor.azure.com", "js2.cdn.applicationinsights.io", "js2.cdn.monitor.azure.com", "az416426.vo.msecnd.net"], o = g.url || cfg.src, r = function () { return s(o, null) }; function s(d, t) { if ((n = navigator) && (~(n = (n.userAgent || "").toLowerCase()).indexOf("msie") || ~n.indexOf("trident/")) && ~d.indexOf("ai.3") && (d = d.replace(/(\/)(ai\.3\.)([^\d]*)$/, function (e, t, n) { return t + "ai.2" + n })), !1 !== cfg.cr) for (var e = 0; e < S.length; e++)if (0 < d.indexOf(S[e])) { y = e; break } var n, i = function (e) { var a, t, n, i, o, r, s, c, u, l; h.queue = [], m || (0 <= y && T + 1 < S.length ? (a = (y + T + 1) % S.length, p(d.replace(/^(.*\/\/)([\w\.]*)(\/.*)$/, function (e, t, n, i) { return t + S[a] + i })), T += 1) : (f = m = !0, s = d, !0 !== cfg.dle && (c = (t = function () { var e, t = {}, n = g.connectionString; if (n) for (var i = n.split(";"), a = 0; a < i.length; a++) { var o = i[a].split("="); 2 === o.length && (t[o[0][x]()] = o[1]) } return t[I] || (e = (n = t.endpointsuffix) ? t.location : null, t[I] = "https://" + (e ? e + "." : "") + "dc." + (n || "services.visualstudio.com")), t }()).instrumentationkey || g.instrumentationKey || "", t = (t = (t = t[I]) && "/" === t.slice(-1) ? t.slice(0, -1) : t) ? t + "/v2/track" : g.endpointUrl, t = g.userOverrideEndpointUrl || t, (n = []).push((i = "SDK LOAD Failure: Failed to load Application Insights SDK script (See stack for details)", o = s, u = t, (l = (r = v(c, "Exception")).data).baseType = "ExceptionData", l.baseData.exceptions = [{ typeName: "SDKLoadFailed", message: i.replace(/\./g, "-"), hasFullStack: !1, stack: i + "\nSnippet failed to load [" + o + "] -- Telemetry is disabled\nHelp Link: https://go.microsoft.com/fwlink/?linkid=2128109\nHost: " + (b && b.pathname || "_unknown_") + "\nEndpoint: " + u, parsedStack: [] }], r)), n.push((l = s, i = t, (u = (o = v(c, "Message")).data).baseType = "MessageData", (r = u.baseData).message = 'AI (Internal): 99 message:"' + ("SDK LOAD Failure: Failed to load Application Insights SDK script (See stack for details) (" + l + ")").replace(/\"/g, "") + '"', r.properties = { endpoint: i }, o)), s = n, c = t, JSON && ((u = C.fetch) && !cfg.useXhr ? u(c, { method: D, body: JSON.stringify(s), mode: "cors" }) : XMLHttpRequest && ((l = new XMLHttpRequest).open(D, c), l.setRequestHeader("Content-type", "application/json"), l.send(JSON.stringify(s))))))) }, a = function (e, t) { m || setTimeout(function () { !t && h.core || i() }, 500), f = !1 }, p = function (e) { var n = O.createElement(q), e = (n.src = e, t && (n.integrity = t), n.setAttribute("data-ai-name", E), cfg[w]); return !e && "" !== e || "undefined" == n[w] || (n[w] = e), n.onload = a, n.onerror = i, n.onreadystatechange = function (e, t) { "loaded" !== n.readyState && "complete" !== n.readyState || a(0, t) }, cfg.ld && cfg.ld < 0 ? O.getElementsByTagName("head")[0].appendChild(n) : setTimeout(function () { O.getElementsByTagName(q)[0].parentNode.appendChild(n) }, cfg.ld || 0), n }; p(d) } cfg.sri && (n = o.match(/^((http[s]?:\/\/.*\/)\w+(\.\d+){1,5})\.(([\w]+\.){0,2}js)$/)) && 6 === n.length ? (d = "".concat(n[1], ".integrity.json"), i = "@".concat(n[4]), l = window.fetch, t = function (e) { if (!e.ext || !e.ext[i] || !e.ext[i].file) throw Error("Error Loading JSON response"); var t = e.ext[i].integrity || null; s(o = n[2] + e.ext[i].file, t) }, l && !cfg.useXhr ? l(d, { method: "GET", mode: "cors" }).then(function (e) { return e.json()["catch"](function () { return {} }) }).then(t)["catch"](r) : XMLHttpRequest && ((a = new XMLHttpRequest).open("GET", d), a.onreadystatechange = function () { if (a.readyState === XMLHttpRequest.DONE) if (200 === a.status) try { t(JSON.parse(a.responseText)) } catch (e) { r() } else r() }, a.send())) : o && r(); try { h.cookie = O.cookie } catch (k) { } function e(e) { for (; e.length;)!function (t) { h[t] = function () { var e = arguments; f || h.queue.push(function () { h[t].apply(h, e) }) } }(e.pop()) } var c, u, l = "track", d = "TrackPage", p = "TrackEvent", l = (e([l + "Event", l + "PageView", l + "Exception", l + "Trace", l + "DependencyData", l + "Metric", l + "PageViewPerformance", "start" + d, "stop" + d, "start" + p, "stop" + p, "addTelemetryInitializer", "setAuthenticatedUserContext", "clearAuthenticatedUserContext", "flush"]), h.SeverityLevel = { Verbose: 0, Information: 1, Warning: 2, Error: 3, Critical: 4 }, (g.extensionConfig || {}).ApplicationInsightsAnalytics || {}); return !0 !== g[L] && !0 !== l[L] && (e(["_" + (c = "onerror")]), u = C[c], C[c] = function (e, t, n, i, a) { var o = u && u(e, t, n, i, a); return !0 !== o && h["_" + c]({ message: e, url: t, lineNumber: n, columnNumber: i, error: a, evt: C.event }), o }, g.autoExceptionInstrumented = !0), h }(cfg.cfg), (C[E] = n).queue && 0 === n.queue.length ? (n.queue.push(e), n.trackPageView({})) : e(); })({
		src: "https://js.monitor.azure.com/scripts/b/ai.3.gbl.min.js",
		crossOrigin: "anonymous",
		cfg: {
			instrumentationKey: "00000000-0000-0000-0000-000000000000",
			disableTelemetry: true
		}
	});
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
	// App Insights */
	//
	if (appDiag && appDiag === "appInsights") {
		loadAppInsights();
	}

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

	// cirreum-bootstrap-XXX.css
	//
	// Allow persisted scheme to override the script attribute
	// (so a user choice survives reloads)
	try {
		const storedScheme = localStorage.getItem("user-theme-name");
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
				localStorage.setItem("user-theme-name", appTheme);
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
	//window.loadCss(
	//	"https://unpkg.com/bootstrap-icons@1.13.1/font/bootstrap-icons.min.css",
	//	"sha256-pdY4ejLKO67E0CM2tbPtq1DJ3VGDVVdqAR6j3ZwdiE4="
	//);


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
	//window.loadJs(
	//	"https://unpkg.com/@popperjs/core@2.11.8/dist/umd/popper.min.js",
	//	"sha384-I7E8VVD/ismYTF4hNIPjVp/Zjvgyol6VFvRkX/vR+Vc4jQkC+hVqc2pM8ODewa9r");

	// draggabilly.js
	// Latest: v3.0.0
	// https://app.unpkg.com/draggabilly@3.0.0
	loadLocalJs("_content/Cirreum.Components.WebAssembly/js/draggabilly.pkgd.min.js");
	//window.loadJs(
	//	"https://unpkg.com/draggabilly@3.0.0/dist/draggabilly.pkgd.min.js",
	//	"sha384-sIOFgJAHSREC1OX+fyKmFycNmzLEWIUXGBkMwi4PII+pOIqCje0nWs3/9Ot2lbzw");

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