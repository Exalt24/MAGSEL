require('dotenv').config();
const express = require('express');
const app = express();
const mongoose = require('mongoose');
const PORT = process.env.PORT || 3000;

DBUSER = process.env.DBUSER;
DBPASS = process.env.DBPASS;

app.use(express.urlencoded({ extended: true }));
app.use(express.json());

app.get('/', (req, res) => {
    console.log('Hello World');
    res.send('Hello World');
});

const userRoutes = require('./src/Routes/UserRoutes');
app.use('/user', userRoutes);

mongoose.connect(`mongodb+srv://${DBUSER}:${DBPASS}@logincluster.xecdzgm.mongodb.net/Metaverse`).then(() => {
    console.log('Connected to the database');

    app.listen(PORT, () => {
        console.log(`Server is running on port ${PORT}`);
    });
}).catch((err) => {
    console.log('Error connecting to the database', err);
});