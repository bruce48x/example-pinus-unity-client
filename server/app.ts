import { pinus } from 'pinus';
import { preload } from './preload';
import { Protobuf } from 'pinus-protobuf-plugin';

/**
 *  替换全局Promise
 *  自动解析sourcemap
 *  捕获全局错误
 */
preload();

/**
 * Init app for client.
 */
let app = pinus.createApp();
app.set('name', 'demo-pinus');

// app configuration
app.configure('production|development', 'connector', function () {
    app.set('connectorConfig',
        {
            connector: pinus.connectors.hybridconnector,
            heartbeat: 3,
            useDict: false,
            useProtobuf: false
        });
    app.use(Protobuf);
});

// start app
app.start();

