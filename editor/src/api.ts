import type { RunRequest, RunResponse, StartResponse, ResetResponse } from "./types";

async function postJson<TResponse>(url: string, body?: unknown): Promise<TResponse> {
  const response = await fetch(url, {
    method: "POST",
    headers: body === undefined ? {} : { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  if (!response.ok) {
    const text = await response.text().catch(() => "");
    throw new Error(`${url} failed: ${response.status} ${text}`);
  }

  return (await response.json()) as TResponse;
}

export const api = {
  start: () => postJson<StartResponse>("/session/start"),
  reset: () => postJson<ResetResponse>("/session/reset"),
  run: (request: RunRequest) => postJson<RunResponse>("/session/run", request),
};
