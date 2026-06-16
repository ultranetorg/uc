// tools/server.js
import http from 'http';

const HOST = process.env.HOST || "127.1.0.100"
const PORT = process.env.PORT || 3160;

const server = http.createServer((req, res) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
  } else if (req.url === '/Ping' && req.method === 'GET') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ Status: 'OK' }));
  } else {
    res.writeHead(404, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ error: 'Not Found' }));
  }
});

server.listen({ host: HOST, port: PORT }, () => {
  console.log(`Server running at http://${HOST}:${PORT}`);
});
