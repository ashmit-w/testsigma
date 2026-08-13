import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Puppet.Host (not built yet) is expected to serve /session/* on this port
// in dev; in production the host serves this app's build output itself, so
// the app talks to /session/* on its own origin and no proxy is involved.
const HOST_DEV_PORT = 5100;

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/session": `http://localhost:${HOST_DEV_PORT}`,
    },
  },
});
