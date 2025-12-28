let mermaidInstance = null;
let isInitialized = false;

export async function initializeMermaid() {
	if (isInitialized) return;

	try {
		// Load Mermaid from CDN
		if (!window.mermaid) {
			const script = document.createElement('script');
			script.src = 'https://unpkg.com/mermaid/dist/mermaid.min.js';
			script.async = true;

			await new Promise((resolve, reject) => {
				script.onload = resolve;
				script.onerror = reject;
				document.head.appendChild(script);
			});
		}

		// Initialize Mermaid
		window.mermaid.initialize({
			startOnLoad: false,
			theme: 'default',
			securityLevel: 'loose',
			flowchart: {
				useMaxWidth: true,
				htmlLabels: true
			},
			sequence: {
				useMaxWidth: true
			},
			gantt: {
				useMaxWidth: true
			}
		});

		mermaidInstance = window.mermaid;
		isInitialized = true;
		console.log('Mermaid initialized successfully');

	} catch (error) {
		console.error('Failed to initialize Mermaid:', error);
		throw error;
	}
}

export async function renderDiagram(diagramDefinition, theme = null) {
	if (!isInitialized || !mermaidInstance) {
		throw new Error('Mermaid not initialized');
	}

	try {
		// Generate unique ID for this diagram
		const diagramId = `mermaid-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

		// Apply theme if specified
		if (theme && theme !== 'default') {
			mermaidInstance.initialize({ theme: theme });
		}

		// Render the diagram and get the SVG
		const { svg } = await mermaidInstance.render(diagramId, diagramDefinition);

		// Reset theme to default if it was changed
		if (theme && theme !== 'default') {
			mermaidInstance.initialize({ theme: 'default' });
		}

		return svg;

	} catch (error) {
		console.error('Failed to render Mermaid diagram:', error);
		throw error;
	}
}

export function clearDiagram(diagramId) {
	// Clean up any diagram-specific resources if needed
	const element = document.getElementById(diagramId);
	if (element) {
		element.innerHTML = '';
	}
}
