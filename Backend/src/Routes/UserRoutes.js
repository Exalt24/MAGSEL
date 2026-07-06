const router = require('express').Router();
const User = require('../Models/User');

const emailRegex = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})?$/;

router.route('/register').post(async (req, res) => {
    const { email, name, password } = req.body;

    // Check that all fields are provided.
    if (!email || !name || !password) {
        return res.status(400).json({ message: 'Please fill in all fields' });
    }
    
    // Trim email and validate format.
    const trimmedEmail = email.trim();
    if (trimmedEmail.length < 1 || !emailRegex.test(trimmedEmail)) {
        return res.status(400).json({ message: 'Please enter a valid email address' });
    }
    
    // Additional validations:
    if (name.trim().length < 2) {
        return res.status(400).json({ message: 'Name must be at least 2 characters long' });
    }
    
    if (password.length < 6) {
        return res.status(400).json({ message: 'Password must be at least 6 characters long' });
    }
    
    try {
        // Check if a user with this email already exists.
        let user = await User.findOne({ email: trimmedEmail });
        if (user) {
            return res.status(400).json({ message: 'Email already exists' });
        }
        
        // Check if a user with this name already exists.
        let nameExists = await User.findOne({ name: name.trim() });
        if (nameExists) {
            return res.status(400).json({ message: 'Username already exists' });
        }
        
        // Create the new user.
        user = { email: trimmedEmail, name: name.trim(), password };
        await User.create(user);
        res.status(201).json({ 
            message: 'User created successfully',
            name: user.name
        });
    } catch (err) {
        console.error('Error registering user', err);
        res.status(500).json({ message: 'Internal Server Error' });
    }
});

router.route('/login').post(async (req, res) => {
    const { email, password } = req.body;
    if (!email || !password) {
        return res.status(400).json({ message: 'Please fill in all fields' });
    }
    try {
        const user = await User.findOne({ email: email.trim() });
        if (!user) {
            return res.status(400).json({ message: 'Invalid credentials' });
        }
        if (password !== user.password) {
            return res.status(400).json({ message: 'Invalid credentials' });
        }
        res.status(200).json({ 
            message: 'User logged in successfully',
            name: user.name 
        });
    } catch (err) {
        console.error('Error logging in user', err);
        res.status(500).json({ message: 'Internal Server Error' });
    }
});

module.exports = router;
