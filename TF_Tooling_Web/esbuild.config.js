require('esbuild').build({
    entryPoints: ['src/TouchFree_Tooling.js', 'src/Plugins/SnappingPlugin/SnappingPlugin.ts'],
    bundle: true,
    minify: false,
    sourcemap: true,
    outdir: 'dist',
  });