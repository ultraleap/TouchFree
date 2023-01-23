## Pull Request Templates

Switch template by going to preview and clicking the link - note it will not work if you've made any changes to the description.

- [default.md](?expand=1) - main template to be used in most situations.
- [lightweight.md](?expand=1&template=lightweight.md) - use for small/minor fixes or changes that don't require significant review.
- [release.md](?expand=1&template=release.md) - use for releases.

**You are currently using: default.md**

Note: these links work by overwriting query parameters of the current url. If the current url contains any you may want to amend the url with `&template=name.md` instead of using the link. See [query parameter docs](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/using-query-parameters-to-create-a-pull-request) for more information.

## Summary

_Summary of the purpose of this pull request._


### Tests Added

_Summary of the tests added as part of this change, or reason why no tests are required._


## Contributor Tasks

_These tasks are for the pull request creator to tick off._

- [ ] PO review (optional depending on work type)
- [ ] XDR review (optional depending on work type)
- [ ] QA review (or another developer if no QA is available)
- [ ] Ensure documentation requirements are met e.g., public API is commented
- [ ] Relevant changelogs have been updated with user-visible changes
    - [ ] [TouchFree Windows](/ultraleap/touchfree/CHANGELOG-windows.md)
    - [ ] [TouchFree BrightSign](/ultraleap/touchfree/CHANGELOG-brightsign.md)
    - [ ] [TouchFree Web Tooling](/ultraleap/touchfree/TF_Tooling_Web/CHANGELOG.md)
- [ ] Consider any licensing/other legal implications e.g., notices required

If there is an associated JIRA issue:
- [ ] Include a link to the JIRA issue in the summary above
- [ ] Make sure the fix version on the issue is set correctly

## Reviewer Tasks

_Add any instructions or tasks for the reviewer such as specific test considerations before this can be merged._

- [ ] Developer testing
- [ ] Code reviewed
- [ ] Non-code assets reviewed
- [ ] Documentation reviewed - includes checking documentation requirements are met and not missing e.g., public API is commented
