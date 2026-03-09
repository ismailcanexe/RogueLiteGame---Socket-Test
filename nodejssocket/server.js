const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const mysql = require('mysql2');

const app = express();
app.use(express.urlencoded({ extended: true }));
app.use(express.json()); 

const server = http.createServer(app);
const io = new Server(server, { cors: { origin: "*" } });

const db = mysql.createPool({
    host: '127.0.0.1',
    user: 'root',
    password: '', 
    database: 'GameDB',
    waitForConnections: true,
    connectionLimit: 10,
    queueLimit: 0
});

db.getConnection((err, connection) => {
    if (err) {
        console.error('VERİTABANI BAĞLANTI HATASI:', err.message);
    } else {
        console.log('Veritabanına başarıyla bağlanıldı.');
        connection.release(); 
    }
});

app.get('/', (req, res) => {
    res.sendFile(__dirname + '/index.html');
});

let players = {}; 

io.on('connection', (socket) => { 
    socket.emit('updateList', Object.values(players));
    socket.on('updateScore', (data) => {
        players[data.username] = data; 
        io.emit('updateList', Object.values(players));
    });
socket.on('aktifisil', (data) => {
    const silinecekIsim = data.username;

    if (players[silinecekIsim]) { 
        delete players[silinecekIsim]; 
        console.log(`Oyuncu listeden kaldırıldı: ${silinecekIsim}`);
        
        io.emit('updateList', Object.values(players));
    } else {
        console.log("Silinecek oyuncu bulunamadı: " + silinecekIsim);
    }
});
});
app.post('/rekor-kaydet', (req, res) => {
    const { oyuncu_adi, skor } = req.body;
    const query = "INSERT INTO scores (oyuncu_adi, skor) VALUES (?, ?)";

    db.query(query, [oyuncu_adi, skor], (err, result) => {
        if (err) {
            console.error("VERİTABANI YAZMA HATASI:", err.message); 
            return res.status(500).send("Veritabanı hatası");
        }
        
        console.log("Skor kaydedildi!");

        io.emit('newRecordAdded'); 

        res.status(200).send("Skor başarıyla kaydedildi!");
    });
});
app.get('/api/rekorlar', (req, res) => {
    const sorgu = "SELECT oyuncu_adi, skor FROM scores ORDER BY skor DESC LIMIT 10";
    
    db.query(sorgu, (err, results) => {
        if (err) {
            console.error("Veri çekme hatası:", err);
            return res.status(500).json({ hata: "Veriler alınamadı." });
        }
        res.json(results);
    });
});

server.listen(3000, () => console.log('Sunucu 3000 portunda yayında!'));