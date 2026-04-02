async function test() {
  try {
    const loginReq = await fetch('http://localhost:5283/api/Auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'admin', password: 'admin123' })
    });
    const { token } = await loginReq.json();
    
    const res = await fetch('http://localhost:5283/api/Auth/reset-requests', {
      headers: { 'Authorization': 'Bearer ' + token }
    });
    const data = await res.json();
    console.log('Status:', res.status);
    console.log('Requests Count:', data.length);
    if (Array.isArray(data)) {
      const processed = data.filter(r => r.isProcessed || r.IsProcessed);
      console.log('Processed in list:', processed.length);
      console.log('Total returned:', data.length);
    } else {
      console.log('Data is not an array:', data);
    }
  } catch (e) { console.error('Test failed:', e); }
}
test();
