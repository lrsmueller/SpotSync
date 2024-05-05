var del = require('del');
var gulp = require('gulp');
var source = '../node\_modules/';
var destination = './wwwroot/npm/';
gulp.task('npm-copy', async function () {
    // tabler
    gulp.src(source + "@tabler/core/dist/\*min\*").pipe(gulp.dest(destination + "/lib/tabler/core"));
});
gulp.task('npm-delete', async function () {
    return del(\[destination + '/\*\*/\*'\]);
});