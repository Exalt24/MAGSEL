require('dotenv').config();
const express = require('express');
const app = express();
const mongoose = require('mongoose');
const PORT = process.env.PORT || 3000;

const DBUSER = process.env.DBUSER;
const DBPASS = process.env.DBPASS;
// MONGO_URI wins when set (CI and local tests point it at a throwaway database); otherwise the Atlas cluster is used as before.
const MONGO_URI = process.env.MONGO_URI || `mongodb+srv://${DBUSER}:${DBPASS}@logincluster.xecdzgm.mongodb.net/Metaverse`;

app.use(express.urlencoded({ extended: true }));
app.use(express.json());

app.get('/', (req, res) => {
    console.log('Hello World');
    res.send('Hello World');
});

const userRoutes = require('./src/Routes/UserRoutes');
app.use('/user', userRoutes);

mongoose.connect(MONGO_URI).then(() => {
    console.log('Connected to the database');

    app.listen(PORT, () => {
        console.log(`Server is running on port ${PORT}`);
    });
}).catch((err) => {
    console.log('Error connecting to the database', err);
});