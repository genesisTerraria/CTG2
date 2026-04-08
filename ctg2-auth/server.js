const express = require('express');
const crypto = require('crypto');
const { createClient } = require('@supabase/supabase-js');
const app = express();

app.use(express.json());

const supabase = createClient(
    process.env.SUPABASE_URL,
    process.env.SUPABASE_ANON_KEY
);

function hash(input) {
    return crypto.createHash('sha256').update(input).digest('hex');
}

// Register
app.post('/register', async (req, res) => {
    const { username, password } = req.body;

    const { data: existing } = await supabase
        .from('users')
        .select('username')
        .eq('username', hash(username))
        .single();

    if (existing) {
        return res.json({ success: false, message: 'Username already taken.' });
    }

    const { error } = await supabase
        .from('users')
        .insert({ username: hash(username), password_hash: hash(password) });

    if (error) {
        return res.json({ success: false, message: 'Database error.' });
    }

    res.json({ success: true });
});

// Login
app.post('/login', async (req, res) => {
    const { username, password } = req.body;

    const { data: user } = await supabase
        .from('users')
        .select('password_hash')
        .eq('username', hash(username))
        .single();

    if (!user) {
        return res.json({ success: false, message: 'No account found.' });
    }

    if (user.password_hash !== hash(password)) {
        return res.json({ success: false, message: 'Wrong password.' });
    }

    res.json({ success: true });
});

// Change password
app.post('/changepassword', async (req, res) => {
    const { username, oldPassword, newPassword } = req.body;

    const { data: user } = await supabase
        .from('users')
        .select('password_hash')
        .eq('username', hash(username))
        .single();

    if (!user)
        return res.json({ success: false, message: 'No account found.' });

    if (user.password_hash !== hash(oldPassword))
        return res.json({ success: false, message: 'Wrong password.' });

    const { error } = await supabase
        .from('users')
        .update({ password_hash: hash(newPassword) })
        .eq('username', hash(username));

    if (error)
        return res.json({ success: false, message: 'Database error.' });

    res.json({ success: true });
});

// Delete account
app.post('/deleteaccount', async (req, res) => {
    const { username, password } = req.body;

    const { data: user } = await supabase
        .from('users')
        .select('password_hash')
        .eq('username', hash(username))
        .single();

    if (!user)
        return res.json({ success: false, message: 'No account found.' });

    if (user.password_hash !== hash(password))
        return res.json({ success: false, message: 'Wrong password.' });

    const { error } = await supabase
        .from('users')
        .delete()
        .eq('username', hash(username));

    if (error)
        return res.json({ success: false, message: 'Database error.' });

    res.json({ success: true });
});

app.listen(3000, () => console.log('Auth server running on port 3000'));