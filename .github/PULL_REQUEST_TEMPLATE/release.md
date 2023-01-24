## Pull Request Templates

Switch template by going to preview and clicking the link - note it will not work if you've made any changes to the description.

- [default.md](?expand=1) - main template to be used in most situations.
- [lightweight.md](?expand=1&template=lightweight.md) - use for small/minor fixes or changes that don't require significant review.
- [release.md](?expand=1&template=release.md) - use for releases.

**You are currently using: release.md**

Note: these links work by overwriting query parameters of the current url. If the current url contains any you may want to amend the url with `&template=name.md` instead of using the link. See [query parameter docs](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/using-query-parameters-to-create-a-pull-request) for more information.

## Release Tasks

_These tasks must be completed to release._

- [ ] Create and request approval of a [Request to Release on Cognidox](https://ultrahaptics.cdox.net/cgi-perl/browse-categories?id=973) for the relevant product being released. This will include several tasks within.
- [ ] Ensure version numbers within software have been updated
- [ ] Relevant changelogs have been updated with user-visible changes
    - [ ] [TouchFree Windows](/ultraleap/touchfree/blob/-/CHANGELOG-windows.md)
    - [ ] [TouchFree BrightSign](/ultraleap/touchfree/blob/-/CHANGELOG-brightsign.md)
    - [ ] [TouchFree Web Tooling](/ultraleap/touchfree/blob/-/TF_Tooling_Web/CHANGELOG.md)
- [ ] Continue release process on [Confluence](https://ultrahaptics.atlassian.net/wiki/spaces/SC/pages/3107979726/TouchFree+Work+Practices#Release-Processes)

## JIRA Release

_Link to the Jira release for this version. See the [releases page](https://ultrahaptics.atlassian.net/projects/TF?selectedItem=com.atlassian.jira.jira-projects-plugin%3Arelease-page)._
