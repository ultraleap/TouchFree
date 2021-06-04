var gulp = require('gulp');
var ts = require('gulp-typescript');
var browserify = require('browserify');
var source = require('vinyl-source-stream');
var buffer = require('vinyl-buffer');

gulp.task('TSC', function () {
    var tsProject = ts.createProject('tsconfig.json');

    var tsResult = tsProject.src()
        .pipe(tsProject());

    return tsResult.js.pipe(gulp.dest('build'));
});

gulp.task('browserify', function () {
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

gulp.task('build',
    gulp.series(
        'TSC',
        'browserify'
    ));