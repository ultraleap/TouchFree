const { Compiler } = require('webpack');

class DtsBundlePlugin {
    apply(compiler) {
        compiler.hooks.done.tapAsync('dTSBundle', (compilation) => {
            var dts = require('dts-bundle');

            dts.bundle({
                name: 'TouchFree',
                main: './build/src/index.d.ts',
                out: '../../dist/TouchFree.d.ts',
                removeSource: false,
                // outputAsModuleFolder: true // to use npm in-package typings
            });
        });
    }
}

module.exports = {
    entry: {
        TouchFree_Tooling: {
            import: './src/index.ts',
            library: {
                name: "TouchFree",
                type: "umd",
            },
            filename: 'dist/TouchFree_Tooling.js'
        },
        Snapping_Plugin: {
            import: './plugins/SnappingPlugin/src/SnappingPlugin.ts',
            dependOn: 'TouchFree_Tooling',
            library: {
                name: "SnappingPlugin",
                type: "umd",
            },
            filename: 'Plugins/SnappingPlugin/Snapping_Plugin.js'
        },
    },
    mode: 'production',
    module: {
        rules: [
            {
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    optimization: {
        minimize: false,
    },
    output: {
        path: __dirname,
    },
    plugins: [new DtsBundlePlugin()],
};