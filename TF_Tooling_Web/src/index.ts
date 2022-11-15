import * as Configuration from './Configuration'
import * as Connection from './Connection'
import * as Cursors from './Cursors'
import * as InputControllers from './InputControllers';
import * as Plugins from './Plugins';
import * as TouchFreeToolingTypes from './TouchFreeToolingTypes';
import * as Tracking from './Tracking';

import TouchFree from './TouchFree';

module.exports = {
    ...TouchFree, // Export all props of the TouchFree object
    Configuration: Configuration,
    Connection: Connection,
    Cursors: Cursors,
    InputControllers: InputControllers,
    Plugins: Plugins,
    TouchFreeToolingTypes: TouchFreeToolingTypes,
    Tracking: Tracking,
}