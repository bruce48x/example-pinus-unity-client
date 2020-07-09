import { Application, FrontendSession } from 'pinus';
import * as Logger from 'pinus-logger';
const logger = Logger.getLogger('server', __filename);
import * as util from 'util';

export default function (app: Application) {
    return new Handler(app);
}

export class Handler {
    constructor(private app: Application) {

    }

    /**
     * New client entry.
     *
     * @param  {Object}   msg     request message
     * @param  {Object}   session current session object
     */
    async entry(msg: any, session: FrontendSession) {
        logger.warn("entry recv", util.inspect(msg, false, 10));
        const code = msg.name ? 0 : 1;
        const rcvmsg = `receive name = ${msg.name}`;
        const i32 = 48;
        const cusMsg = { i32: 9, i32arr: [7, 5, 3] };
        const msgArr = [
            { i32: 1, i32arr: [1000, 2000, 3000] },
            { i32: 2, i32arr: [90, 80, 70] },
            { i32: 3, i32arr: [1, 2, 3] },
        ];
        const flArr = [1.1, 2.2, 3.3];
        const dbArr = [0.3, 0.2, 0.1];
        return {
            code,
            i32,
            rcvmsg,
            flArr,
            dbArr,
            cusMsg,
            msgArr,
        };
    }

    /**
     * Publish route for mqtt connector.
     *
     * @param  {Object}   msg     request message
     * @param  {Object}   session current session object
     */
    async publish(msg: any, session: FrontendSession) {
        let result = {
            topic: 'publish',
            payload: JSON.stringify({ code: 200, msg: 'publish message is ok.' })
        };
        return result;
    }

    /**
     * Subscribe route for mqtt connector.
     *
     * @param  {Object}   msg     request message
     * @param  {Object}   session current session object
     */
    async subscribe(msg: any, session: FrontendSession) {
        let result = {
            topic: 'subscribe',
            payload: JSON.stringify({ code: 200, msg: 'subscribe message is ok.' })
        };
        return result;
    }

}