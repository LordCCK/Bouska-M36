const { app, BrowserWindow, ipcMain, dialog } = require('electron');
const path = require('path');
const { spawn } = require('child_process');
const fetch = require('node-fetch');

let mainWindow;
let backendProcess;

function createWindow() {
    mainWindow = new BrowserWindow({
        width: 1200,
        height: 800,
        webPreferences: {
            nodeIntegration: true,
            contextIsolation: false,
            enableRemoteModule: true
        },
        icon: path.join(__dirname, '../assets/icon.png'),
        show: false
    });

    mainWindow.loadFile('src/index.html');
    
    mainWindow.once('ready-to-show', () => {
        mainWindow.show();
        // Start fullscreen card reader mode
        mainWindow.maximize();
    });

    mainWindow.on('closed', () => {
        mainWindow = null;
        if (backendProcess) {
            backendProcess.kill();
        }
    });

    // Development mode
    if (process.argv.includes('--dev')) {
        mainWindow.webContents.openDevTools();
    }
}

function startBackend() {
    const backendPath = path.join(__dirname, '../backend/M36Backend/bin/Release/net6.0/M36Backend.exe');
    
    try {
        backendProcess = spawn(backendPath, [], {
            stdio: 'pipe'
        });

        backendProcess.stdout.on('data', (data) => {
            console.log(`Backend: ${data}`);
        });

        backendProcess.stderr.on('data', (data) => {
            console.error(`Backend Error: ${data}`);
        });

        backendProcess.on('close', (code) => {
            console.log(`Backend process exited with code ${code}`);
        });
    } catch (error) {
        console.error('Failed to start backend:', error);
    }
}

app.whenReady().then(() => {
    createWindow();
    startBackend();

    app.on('activate', () => {
        if (BrowserWindow.getAllWindows().length === 0) {
            createWindow();
        }
    });
});

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

// IPC handlers for communication with renderer process
ipcMain.handle('get-orders', async () => {
    try {
        const response = await fetch('http://localhost:5000/api/orders');
        const orders = await response.json();
        return orders;
    } catch (error) {
        console.error('Error fetching orders:', error);
        return [];
    }
});

ipcMain.handle('get-order-details', async (event, orderNumber) => {
    try {
        const response = await fetch(`http://localhost:5000/api/orders/${orderNumber}`);
        const orderDetails = await response.json();
        return orderDetails;
    } catch (error) {
        console.error('Error fetching order details:', error);
        return {};
    }
});

ipcMain.handle('save-test-result', async (event, testData) => {
    try {
        const response = await fetch('http://localhost:5000/api/orders/test-result', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(testData)
        });
        const result = await response.json();
        return result;
    } catch (error) {
        console.error('Error saving test result:', error);
        return { success: false, error: error.message };
    }
});

ipcMain.handle('print-labels', async (event, printData) => {
    try {
        const response = await fetch('http://localhost:5000/api/print/labels', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(printData)
        });
        const result = await response.json();
        return result;
    } catch (error) {
        console.error('Error printing labels:', error);
        return { success: false, error: error.message };
    }
});
