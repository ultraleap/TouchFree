import { Address, ConnectionManager } from '../ConnectionManager';

describe('ConnectionManager', () => {
    const checkAddress = (address: Address) => {
        expect(ConnectionManager.iPAddress).toBe(address.ip);
        expect(ConnectionManager.port).toBe(address.port);
    };
    it("should update the instance's port and ip when SetAddress is called", () => {
        const newAddress = { ip: '192.168.0.1', port: '8080' };
        ConnectionManager.init();
        checkAddress({ ip: '127.0.0.1', port: '9739' });
        ConnectionManager.SetAddress(newAddress);
        checkAddress(newAddress);
    });

    it("should set the instance's port and ip from an init call", () => {
        const newAddress = { ip: '192.168.0.2', port: '9090' };
        ConnectionManager.init({ address: newAddress });
        checkAddress(newAddress);
    });
});
