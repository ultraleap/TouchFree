const path = require('path');

module.exports = {
    entry: {
        'touchfree-tooling': './src/index.ts',
        'snapping-plugin': './Plugins/SnappingPlugin/src/SnappingPlugin.ts',
    },
    mode: 'production',
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: '[name].js',
        library: '[name]',
        libraryTarget: 'umd',
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js'],
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
    },
    optimization: {
        minimize: false,
    },
};
