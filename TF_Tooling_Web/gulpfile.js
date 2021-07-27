var gulp = require('gulp');
var ts = require('gulp-typescript');
var browserify = require('browserify');
var source = require('vinyl-source-stream');
var buffer = require('vinyl-buffer');
var merge = require('merge-stream');

gulp.task('compile_tooling', function () {
    var tsProject = ts.createProject('tsconfig.json');

    var tsResult =  tsProject.src().pipe(tsProject());

    return merge(tsResult, tsResult.js)
        .pipe(gulp.dest('./build'));
});

gulp.task('compile_snapping', function () {
    var tsProject = ts.createProject('./examples/SnappingPlugin/tsconfig.json');

    var tsResult =  tsProject.src().pipe(tsProject());

    return merge(tsResult, tsResult.js)
        .pipe(gulp.dest('./build/examples/SnappingPlugin'));
});

gulp.task('browserify_tooling', function () {
    var b = browserify({
        standalone: "TouchFree"
    });
    b.add('./build/src/');

    return b.bundle()
        // log errors if they happen
        .on('error', (error) => { console.error(error); })
        .pipe(source('TouchFree_Tooling.js'))
        .pipe(buffer())
        .pipe(gulp.dest('./dist/'));
});

gulp.task('browserify_snapping', function () {
    var b = browserify({
        standalone: "TouchFree"
    });
    b.add('./build/examples/SnappingPlugin/');

    return b.bundle()
        // log errors if they happen
        .on('error', (error) => { console.error(error); })
        .pipe(source('Snapping_Plugin.js'))
        .pipe(buffer())
        .pipe(gulp.dest('./dist/'));
});

gulp.task('build',
    gulp.series(
        'compile_tooling',
        'compile_snapping',
        'browserify_tooling'
    ));