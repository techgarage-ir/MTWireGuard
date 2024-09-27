class APIClient {
    constructor() { }

    extractFilenameFromHeader(headers) {
        const contentDisposition = headers.get('Content-Disposition');
        if (contentDisposition && contentDisposition.includes('filename=')) {
            return contentDisposition.split('filename=')[1];
        }
        return 'downloaded-file';
    }

    async makeRequest(endpoint, method = 'GET', body = null, download = false) {
        try {
            if (download) {
                return endpoint;
            } else {
                const response = await fetch(endpoint, {
                    method: method,
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: body ? JSON.stringify(body) : null
                });

                const result = await response.json();
                return result;
            }
        } catch (err) {
            throw err;
        }
    }

    auth = {
        login: async (username, password) => {
            return fetch(this.endpoints.auth.Login(), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    username,
                    password
                })
            }).then((response) => {
                if (response.status == 200) {
                    location.reload();
                    return true;
                }
                console.error(`Error: ${response.status}`, `Message: ${response.text()}`);
                return false;
            }).catch((err) => {
                console.log(err);
                throw err;
            });
        },
        logout: () => this.makeRequest(this.endpoints.auth.Logout)
    };

    users = {
        getAll: () => this.makeRequest(this.endpoints.Users),
        getOnlines: () => this.makeRequest(this.endpoints.users.Onlines()),
        getCount: () => this.makeRequest(this.endpoints.users.Count()),
        get: (userId) => this.makeRequest(this.endpoints.users.Single(userId)),
        create: (user) => this.makeRequest(this.endpoints.Users, 'POST', user),
        update: (userId, updatedUser) => this.makeRequest(this.endpoints.users.Single(userId), 'PUT', updatedUser),
        sync: (userId, updatedUser) => this.makeRequest(this.endpoints.users.Sync(userId), 'PATCH', updatedUser),
        qr: (userId) => this.makeRequest(this.endpoints.users.QR(userId)),
        download: (userId) => this.makeRequest(this.endpoints.users.Download(userId), 'GET', null, true),
        delete: (userId) => this.makeRequest(this.endpoints.users.Single(userId), 'DELETE'),
        activate: (userId, isEnabled) => this.makeRequest(this.endpoints.users.Activation(userId), 'PATCH', isEnabled)
    };

    servers = {
        getAll: () => this.makeRequest(this.endpoints.Servers),
        getCount: () => this.makeRequest(this.endpoints.servers.Count()),
        get: (serverId) => this.makeRequest(this.endpoints.servers.Single(serverId)),
        create: (server) => this.makeRequest(this.endpoints.Servers, 'POST', server),
        update: (serverId, updatedServer) => this.makeRequest(this.endpoints.servers.Single(serverId), 'PUT', updatedServer),
        delete: (serverId) => this.makeRequest(this.endpoints.servers.Single(serverId), 'DELETE'),
        activate: (serverId, isEnabled) => this.makeRequest(this.endpoints.servers.Activation(serverId), 'PATCH', isEnabled)
    };

    pools = {
        getAll: () => this.makeRequest(this.endpoints.Pools),
        create: (pool) => this.makeRequest(this.endpoints.Pools, 'POST', pool),
        update: (poolId, updatedPool) => this.makeRequest(this.endpoints.pools.Single(poolId), 'PUT', updatedPool),
        delete: (poolId) => this.makeRequest(this.endpoints.pools.Single(poolId), 'DELETE')
    };

    config = {
        dns: {
            get: () => this.makeRequest(this.endpoints.config.DNS),
            update: (updatedDNS) => this.makeRequest(this.endpoints.config.DNS, 'PUT', updatedDNS)
        },
        identity: {
            get: () => this.makeRequest(this.endpoints.config.Identity),
            update: (updatedIdentity) => this.makeRequest(this.endpoints.config.Identity, 'PUT', updatedIdentity)
        },
        logs: {
            getAll: () => this.makeRequest(this.endpoints.config.Logs)
        },
        resources: {
            get: () => this.makeRequest(this.endpoints.config.Resources)
        },
        information: {
            get: () => this.makeRequest(this.endpoints.config.Information)
        }
    }

    endpoints = (function () {
        const baseUrl = "/api";
        const auth = `${baseUrl}/auth`;
        const users = `${baseUrl}/users`;
        const servers = `${baseUrl}/servers`;
        const pools = `${baseUrl}/ippools`;
        const config = `${baseUrl}/config`;

        return {
            Auth: auth,
            Users: users,
            Servers: servers,
            Pools: pools,

            auth: {
                Login: () => { return `${auth}/login`; },
                Logout: () => { `${auth}/logout`; },
            },
            users: {
                Single: (userId) => { return `${users}/${userId}`; },
                Sync: (userId) => { return `${users}/sync/${userId}`; },
                QR: (userId) => { return `${users}/qr/${userId}`; },
                Download: (userId) => { return `${users}/file/${userId}`; },
                Activation: (userId) => { return `${users}/activation/${userId}`; },
                Onlines: () => { return `${users}/onlines` },
                Count: () => { return `${users}/count` }
            },
            servers: {
                Single: (serverId) => { return `${servers}/${serverId}`; },
                Activation: (serverId) => { return `${servers}/activation/${serverId}`; },
                Count: () => { return `${servers}/count` }
            },
            pools: {
                Single: (poolId) => { return `${pools}/${poolId}`; },
            },
            config: {
                DNS: `${config}/dns`,
                Identity: `${config}/identity`,
                Information: `${config}/information`,
                Logs: `${config}/logs`,
                Resources: `${config}/resources`
            }
        };
    })();
}
