## Bug: HTTP/REST clients cannot maintain MCP session with curl

### Pre-flight Checks
- [x] I have searched existing issues and this is not a duplicate
- [x] I understand this issue needs status:approved before a PR can be opened

---

### Bug Description

When using the HTTP endpoint (`http://127.0.0.1:9000/mcp`) with standard HTTP clients like `curl`, the MCP server returns `"Missing session ID"` errors on subsequent requests after the initial `initialize` call succeeds.

The first request (`initialize`) works and returns a valid response. But any following request (like `tools/call`, `resources/list`, or even the `initialized` notification) fails with:

```
{"jsonrpc":"2.0","id":"server-error","error":{"code":-32600,"message":"Bad Request: Missing session ID"}}
```

This makes it impossible to use the MCP server from any HTTP-based client that doesn't natively support SSE/websockets.

---

### Steps to Reproduce

1. Start Unity with MCP for Unity server running on port 9000
2. Send initialize request via curl:
```bash
curl -N -X POST "http://127.0.0.1:9000/mcp" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
```

3. Observe successful response with server info

4. Send any follow-up request (e.g., `initialized` notification or `tools/call`):
```bash
curl -N -X POST "http://127.0.0.1:9000/mcp" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -d '{"jsonrpc":"2.0","id":null,"method":"initialized","params":{}}'
```

5. Observe error: `"Bad Request: Missing session ID"`

---

### Expected Behavior

HTTP clients should be able to maintain a session via:
- Cookies (Set-Cookie header)
- URL-based session tokens
- Authorization headers with session tokens
- Or documented mechanism for stateless clients

---

### Actual Behavior

The server rejects all requests after initialize with "Missing session ID". The session mechanism appears to rely on the SSE connection staying open, which is not compatible with standard REST/HTTP patterns.

---

### Operating System
Windows

### Agent / Client
OpenCode (but issue affects any HTTP client)

### Shell
PowerShell / bash

---

### Relevant Logs

```
# Request 1 - WORKS
> POST /mcp HTTP/1.1
> Host: 127.0.0.1:9000
> Content-Type: application/json
> Accept: application/json, text/event-stream
> 
< HTTP/1.1 200 OK
< event: message
< data: {"jsonrpc":"2.0","id":1,"result":{"serverInfo":{"name":"mcp-for-unity-server","version":"3.2.2"},...}}

# Request 2 - FAILS
> POST /mcp HTTP/1.1
> Host: 127.0.0.1:9000
< HTTP/1.1 400 Bad Request
< data: {"jsonrpc":"2.0","id":"server-error","error":{"code":-32600,"message":"Bad Request: Missing session ID"}}
```

---

### Additional Context

This is blocking integration with OpenCode agents that need to make isolated tool calls. Clients like `curl`, Python `requests`, or any HTTP library cannot work with this MCP server unless they maintain a persistent SSE connection.

Possible solutions:
1. Add cookie-based session tracking for HTTP clients
2. Add URL parameter support for session tokens (e.g., `/mcp?session=xyz`)
3. Document the expected SSE/websocket flow for HTTP clients
4. Add a simple REST endpoint alternative for stateless calls

---

### Related Issues

- #1049: "要怎么用openclaw来连上这个mcp服务器啊" (Chinese - similar connection issue)
- #940: "Cannot connect to the MCP server via vscode plugin"
- #933: "codex as a client"
