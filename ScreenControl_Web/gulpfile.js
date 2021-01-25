var gulp = require('gulp');
var ts = require('gulp-typescript');
var browserify = require('browserify');
var source = require('vinyl-source-stream');
var buffer = require('vinyl-buffer');

const { stdout } = require('process');

var b = browserify({
    standalone: "TestModule"
});

gulp.task('TSC', function () {
    var tsProject = ts.createProject('tsconfig.json');

    var tsResult = tsProject.src()
        .pipe(tsProject());

    return tsResult.js.pipe(gulp.dest('build'));
});

gulp.task('browserify', function () {
    b.add('./build/src/TestModule.js');

    return b.bundle()
        // log errors if they happen
        .on('error', (error) => { console.error(error); })
        .pipe(source('TestModule.js'))
        .pipe(buffer())
        .pipe(gulp.dest('./build/dist/'));
});

gulp.task('build',
    gulp.series(
        'TSC',
        'browserify'
    ));