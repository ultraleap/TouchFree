const path = require('path');

module.exports = {
    entry: {
        TouchFree_Tooling: {
            import: './src/index.ts',
            library: {
                name: "TouchFree",
                type: "umd",
            },
        },
        Snapping_Plugin: {
            import: './examples/SnappingPlugin/src/SnappingPlugin.ts',
            dependOn: 'TouchFree_Tooling',
            library: {
                name: "SnappingPlugin",
                type: "umd",
            },
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
        filename: '[name].js',
        path: path.resolve(__dirname, 'dist'),
    },
};