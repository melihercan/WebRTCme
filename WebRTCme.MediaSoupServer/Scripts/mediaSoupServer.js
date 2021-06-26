const server = require('./server');

module.exports = {
    runServer: async () => {
        // No need to call this explicitly, refering to 'server' loads server.js and executes the 'run' function. 
        //await server.run();
    }
}