/* jshint esversion: 6 */
const request = require('request');

const connect = (config) => {
    return (query, callback) => {
        let queryString = 'https://' + config.searchName + '.search.windows.net/indexes/' + config.indexName + '/docs?api-key=' + config.searchKey + '&api-version=2015-02-28&' + query;
        request(queryString, (error, response, body) => {
            if (!error && response && response.statusCode == 200) {
                var result = JSON.parse(body);
                callback(null, result);
            } else {
                callback(error, null);
            }
        });
    };
};

module.exports = connect;
