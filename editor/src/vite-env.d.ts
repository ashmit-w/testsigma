/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_EXE_PATH?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
