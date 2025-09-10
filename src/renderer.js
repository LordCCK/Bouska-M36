const { ipcRenderer } = require('electron');
const { SerialPort } = require('serialport');
const { ReadlineParser } = require('@serialport/parser-readline');

class M36Application {
    constructor() {
        this.currentUser = '';
        this.currentOrder = null;
        this.selectedPart = '';
        this.serialPort = null;
        this.testInProgress = false;
        this.testData = [];
        this.leakValues = [];
        this.setpoint = 0;
        
        this.initializeEventListeners();
        this.initializeSerialPort();
    }

    initializeEventListeners() {
        // Načtení karty - poslouchání klávesnice
        document.addEventListener('keydown', (e) => {
            if (document.getElementById('cardReaderScreen').style.display !== 'none') {
                this.handleCardInput(e);
            } else {
                this.handleBarcodeInput(e);
            }
        });

        // Refresh zakázek
        document.getElementById('refreshBtn').addEventListener('click', () => {
            this.refreshOrders();
        });

        // Výběr zakázky
        document.getElementById('orderSelect').addEventListener('change', (e) => {
            if (e.target.value) {
                this.loadOrderDetails(e.target.value);
            }
        });

        // Tisk etiket
        document.getElementById('printBtn').addEventListener('click', () => {
            this.showPrintDialog();
        });

        // Uživatelské info - návrat na úvodní obrazovku
        document.querySelector('.user-info').addEventListener('click', () => {
            this.returnToCardReader();
        });

        // Print dialog events
        document.getElementById('closePrintDialog').addEventListener('click', () => {
            this.hidePrintDialog();
        });

        document.getElementById('printAllBtn').addEventListener('click', () => {
            this.printAllLabels();
        });

        document.getElementById('singlePartSelect').addEventListener('change', (e) => {
            document.getElementById('printSingleBtn').disabled = !e.target.value;
        });

        document.getElementById('printSingleBtn').addEventListener('click', () => {
            this.printSingleLabel();
        });
    }

    async initializeSerialPort() {
        try {
            // Najdeme sériový port detektoru (9600 8b)
            const ports = await SerialPort.list();
            const detectorPort = ports.find(port => port.manufacturer && 
                port.manufacturer.toLowerCase().includes('detector'));

            if (detectorPort) {
                this.serialPort = new SerialPort({
                    path: detectorPort.path,
                    baudRate: 9600,
                    dataBits: 8,
                    parity: 'none',
                    stopBits: 1
                });

                const parser = this.serialPort.pipe(new ReadlineParser({ delimiter: '\r\n' }));
                parser.on('data', (data) => {
                    this.handleSerialData(data);
                });

                console.log(`Připojen k detektoru na portu: ${detectorPort.path}`);
            }
        } catch (error) {
            console.error('Chyba při inicializaci sériového portu:', error);
        }
    }

    handleCardInput(e) {
        if (e.key === 'Enter') {
            // Simulace načtení karty - v reálné implementaci by zde byla logika čtečky
            const cardData = document.querySelector('input[type="text"]:focus')?.value || 'TestUser';
            this.currentUser = cardData;
            this.showMainScreen();
        }
    }

    handleBarcodeInput(e) {
        if (e.key === 'Enter') {
            // Zpracování načtení čárového kódu dílu
            const barcodeInput = document.querySelector('input[type="text"]:focus');
            if (barcodeInput && barcodeInput.value) {
                this.selectPart(barcodeInput.value);
                barcodeInput.value = '';
            }
        }
    }

    showMainScreen() {
        document.getElementById('cardReaderScreen').style.display = 'none';
        document.getElementById('mainScreen').style.display = 'block';
        document.getElementById('userName').textContent = this.currentUser;
        
        // Automatický refresh zakázek při spuštění
        this.refreshOrders();
    }

    returnToCardReader() {
        document.getElementById('mainScreen').style.display = 'none';
        document.getElementById('cardReaderScreen').style.display = 'flex';
        this.currentUser = '';
        this.currentOrder = null;
        this.resetInterface();
    }

    async refreshOrders() {
        try {
            const orders = await ipcRenderer.invoke('get-orders');
            const select = document.getElementById('orderSelect');
            
            // Vyčistit existující možnosti
            select.innerHTML = '<option value="">Vyberte zakázku...</option>';
            
            // Přidat zakázky do selectu
            orders.forEach(order => {
                const option = document.createElement('option');
                option.value = order.number;
                option.textContent = `${order.number} - ${order.description}`;
                select.appendChild(option);
            });
        } catch (error) {
            console.error('Chyba při načítání zakázek:', error);
        }
    }

    async loadOrderDetails(orderNumber) {
        try {
            const orderDetails = await ipcRenderer.invoke('get-order-details', orderNumber);
            this.currentOrder = orderDetails;
            
            // Zobrazit informace o zakázce v hlavičce
            document.getElementById('orderInfo').style.display = 'block';
            document.getElementById('orderNumber').textContent = `Zakázka: ${orderDetails.number}`;
            document.getElementById('orderType').textContent = `Typ: ${orderDetails.type}`;
            document.getElementById('orderPCN').textContent = `PCN: ${orderDetails.pcn}`;
            
            // Zobrazit tlačítko tisku
            document.getElementById('printBtn').style.display = 'block';
            
            // Načíst tabulku výrobků
            this.loadProductsTable(orderDetails.products);
            
        } catch (error) {
            console.error('Chyba při načítání detailů zakázky:', error);
        }
    }

    loadProductsTable(products) {
        const tbody = document.getElementById('productsTableBody');
        tbody.innerHTML = '';
        
        products.forEach(product => {
            const row = document.createElement('tr');
            row.dataset.partNumber = product.partNumber;
            
            row.innerHTML = `
                <td>${product.partNumber}</td>
                <td><span class="status-pending">Čeká</span></td>
                <td><span class="status-pending">Čeká</span></td>
                <td><span class="status-pending">Nenačteno</span></td>
            `;
            
            tbody.appendChild(row);
        });
    }

    selectPart(partNumber) {
        // Označit vybraný díl v tabulce
        const rows = document.querySelectorAll('#productsTableBody tr');
        rows.forEach(row => {
            row.classList.remove('selected');
            if (row.dataset.partNumber === partNumber) {
                row.classList.add('selected');
                // Označit funkční test jako OK
                const functionalTestCell = row.children[1];
                functionalTestCell.innerHTML = '<span class="status-ok">OK</span>';
            }
        });
        
        this.selectedPart = partNumber;
        console.log(`Vybrán díl: ${partNumber}`);
    }

    handleSerialData(data) {
        const serialTextArea = document.getElementById('SerialPortData');
        serialTextArea.value += data;
        
        // Zpracování dat podle vaší logiky
        let val1 = "";
        try { 
            val1 = data.replace(/\r?\n|\r/g, ""); 
        } catch (e) {
            console.error('Chyba při zpracování dat:', e);
            return;
        }

        if (serialTextArea.value.length > 20) {
            const v = serialTextArea.value;
            
            if (v.indexOf('TEST') !== -1 || v.indexOf('ROUGH') !== -1) {
                this.startTest();
                document.getElementById('hiddSerial').value = val1;
                document.getElementById('testInfo').innerText = val1;
                document.getElementById('barcodeLabel').innerText = this.selectedPart;
                this.testInProgress = true;
            }

            if (v.indexOf('STAND-BY') !== -1 && v.indexOf('PASS') !== -1) {
                if (this.testInProgress) {
                    this.endTest();
                    setTimeout(() => this.showProductsTable(), 3500);
                }
            }
        }

        // Ukládání hodnot úniku pro průměr
        const leakMatch = data.match(/LEAK:\s*([\d.]+)/);
        if (leakMatch) {
            const leakValue = parseFloat(leakMatch[1]);
            this.leakValues.push(leakValue);
            
            // Udržovat pouze posledních 5 hodnot
            if (this.leakValues.length > 5) {
                this.leakValues.shift();
            }
            
            this.updateTestDisplay(leakValue);
        }

        // Nastavená hodnota
        const setpointMatch = data.match(/SETPOINT:\s*([\d.]+)/);
        if (setpointMatch) {
            this.setpoint = parseFloat(setpointMatch[1]);
            document.getElementById('setpointValue').textContent = this.setpoint.toFixed(3);
        }
    }

    startTest() {
        document.getElementById('productsTable').style.display = 'none';
        document.getElementById('testArea').style.display = 'block';
        this.leakValues = [];
    }

    endTest() {
        this.testInProgress = false;
        
        if (this.leakValues.length >= 5) {
            const average = this.leakValues.reduce((a, b) => a + b, 0) / this.leakValues.length;
            const result = average < this.setpoint ? 'PASS' : 'FAIL';
            
            // Uložit výsledek testu
            this.saveTestResult(this.selectedPart, average, result);
            
            // Aktualizovat tabulku
            this.updatePartStatus(this.selectedPart, result);
        }
        
        // Vyčistit zobrazení
        document.getElementById('barcodeLabel').innerText = '';
        document.getElementById('SerialPortData').value = '';
    }

    updateTestDisplay(currentLeak) {
        document.getElementById('currentLeak').textContent = currentLeak.toFixed(3);
        
        if (this.leakValues.length > 0) {
            const average = this.leakValues.reduce((a, b) => a + b, 0) / this.leakValues.length;
            document.getElementById('averageLeak').textContent = average.toFixed(3);
        }
    }

    showProductsTable() {
        document.getElementById('testArea').style.display = 'none';
        document.getElementById('productsTable').style.display = 'block';
    }

    async saveTestResult(partNumber, leakValue, result) {
        try {
            const testData = {
                order: this.currentOrder.number,
                barcode: partNumber,
                date: new Date().toLocaleDateString('cs-CZ'),
                time: new Date().toLocaleTimeString('cs-CZ'),
                operator: this.currentUser,
                setpoint: this.setpoint,
                leak: leakValue,
                result: result,
                pcn: this.currentOrder.pcn,
                type: this.currentOrder.type
            };
            
            await ipcRenderer.invoke('save-test-result', testData);
            console.log('Výsledek testu uložen:', testData);
        } catch (error) {
            console.error('Chyba při ukládání výsledku testu:', error);
        }
    }

    updatePartStatus(partNumber, result) {
        const rows = document.querySelectorAll('#productsTableBody tr');
        rows.forEach(row => {
            if (row.dataset.partNumber === partNumber) {
                const heTestCell = row.children[2];
                const statusCell = row.children[3];
                
                if (result === 'PASS') {
                    heTestCell.innerHTML = '<span class="status-ok">OK</span>';
                    statusCell.innerHTML = '<span class="status-ok">Dokončeno</span>';
                    // Odstranit řádek z tabulky
                    setTimeout(() => row.remove(), 2000);
                } else {
                    heTestCell.innerHTML = '<span class="status-fail">FAIL</span>';
                    statusCell.innerHTML = '<span class="status-fail">Chyba</span>';
                }
            }
        });
    }

    showPrintDialog() {
        // Naplnit select s díly
        const singlePartSelect = document.getElementById('singlePartSelect');
        singlePartSelect.innerHTML = '<option value="">Vyberte díl pro jednotlivý tisk...</option>';
        
        if (this.currentOrder && this.currentOrder.products) {
            this.currentOrder.products.forEach(product => {
                const option = document.createElement('option');
                option.value = product.partNumber;
                option.textContent = product.partNumber;
                singlePartSelect.appendChild(option);
            });
        }
        
        document.getElementById('printDialog').style.display = 'flex';
    }

    hidePrintDialog() {
        document.getElementById('printDialog').style.display = 'none';
    }

    async printAllLabels() {
        try {
            await ipcRenderer.invoke('print-labels', {
                type: 'all',
                order: this.currentOrder
            });
            this.hidePrintDialog();
        } catch (error) {
            console.error('Chyba při tisku všech etiket:', error);
        }
    }

    async printSingleLabel() {
        const selectedPart = document.getElementById('singlePartSelect').value;
        if (!selectedPart) return;
        
        try {
            await ipcRenderer.invoke('print-labels', {
                type: 'single',
                partNumber: selectedPart,
                order: this.currentOrder
            });
            this.hidePrintDialog();
        } catch (error) {
            console.error('Chyba při tisku etikety:', error);
        }
    }

    resetInterface() {
        document.getElementById('orderSelect').selectedIndex = 0;
        document.getElementById('orderInfo').style.display = 'none';
        document.getElementById('printBtn').style.display = 'none';
        document.getElementById('productsTableBody').innerHTML = '';
        document.getElementById('testArea').style.display = 'none';
        document.getElementById('productsTable').style.display = 'block';
    }
}

// Inicializace aplikace po načtení DOM
document.addEventListener('DOMContentLoaded', () => {
    window.m36App = new M36Application();
});

// Přidání skrytého input pole pro načítání čárových kódů
document.addEventListener('DOMContentLoaded', () => {
    const hiddenInput = document.createElement('input');
    hiddenInput.type = 'text';
    hiddenInput.style.position = 'absolute';
    hiddenInput.style.left = '-9999px';
    hiddenInput.style.opacity = '0';
    document.body.appendChild(hiddenInput);
    
    // Udržovat focus na skrytém input poli
    document.addEventListener('click', () => {
        if (document.getElementById('cardReaderScreen').style.display === 'none') {
            hiddenInput.focus();
        }
    });
    
    setInterval(() => {
        if (document.getElementById('cardReaderScreen').style.display === 'none') {
            hiddenInput.focus();
        }
    }, 1000);
});
