/**
 * @packageDocumentation
 * TouchFree is an ecosystem of software products for enabling
 * touchless interfaces. This package is a client package
 * for integrating TouchFree into web application.
 * See https://docs.ultraleap.com/touchfree-user-manual/
 */

import * as Configuration from './Configuration';
import * as Connection from './Connection';
import * as Cursors from './Cursors';
import * as InputControllers from './InputControllers';
import * as Plugins from './Plugins';
import * as Tracking from './Tracking';
import * as TouchFreeToolingTypes from './TouchFreeToolingTypes';
import * as Utilities from './Utilities';
import TF, * as TouchFree from './TouchFree';

export
{
    Configuration,
    Connection,
    Cursors,
    InputControllers,
    Plugins,
    Tracking,
    TouchFreeToolingTypes,
    Utilities,
    TouchFree
};
export default TF;