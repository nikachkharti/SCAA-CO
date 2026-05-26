import { API_BASE_URL } from "./config.js";

export async function apiRequest(
    endpoint,
    method = "GET",
    body = null
) {
    const options = {
        method,
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        }
    };

    if (body !== null) {
        options.body = JSON.stringify(body);
    }

    let response;
    try {
        response = await fetch(
            `${API_BASE_URL}${endpoint}`,
            options
        );
    }
    catch {
        throw new Error("Network error — is the API running?");
    }

    if (response.status === 204) {
        return null;
    }

    const text = await response.text();
    const payload = text ? safeJsonParse(text) : null;

    if (!response.ok) {
        const message =
            (payload && payload.message) ||
            text ||
            `Request failed with status ${response.status}`;
        throw new Error(message);
    }

    return payload && "result" in payload ? payload.result : payload;
}

function safeJsonParse(text) {
    try {
        return JSON.parse(text);
    }
    catch {
        return null;
    }
}
