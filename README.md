# 🕹️ CyberZone - Cyber Cafe Management System

CyberZone is a comprehensive web application designed to automate and streamline the operations of a modern cyber cafe. It handles PC reservations, dynamic time-billing, real-time session tracking, and bar inventory management. 

The system provides tailored interfaces and permissions for three distinct user roles: Gamers, Shift Admins, and Cafe Owners.

## ✨ Core Features

### 👨‍💻 For Gamers (Users)
* **Interactive Seating Map:** Real-time visual representation of PC statuses (Available, Occupied, Booked).
* **Self-Service Booking:** Reserve a PC in a specific zone without admin intervention.
* **Balance Management:** Check account balance and track active session time.

### 🛡️ For Shift Admins
* **Session Control:** Start, pause, or terminate user PC sessions.
* **Bar Inventory:** Sell snacks/drinks (e.g., Snickers, Cola) and deduct items from the inventory.
* **Issue Resolution:** Override bookings or adjust time for hardware troubleshooting.

### 👑 For Owners
* **Dynamic Pricing Engine:** Configure hourly rates based on zones (e.g., Standard GTX 1650 vs. VIP RTX 4090) and time slots (e.g., Fixed-price "Night Mode" from 22:00 to 06:00).
* **Financial Dashboard:** Track daily revenue, PC utilization rates, and bar sales.

## ⚙️ Key Technical Use Cases Implemented
1. **State Machine:** Managing PC statuses (`AVAILABLE` -> `BOOKED` -> `IN_USE` -> `MAINTENANCE`).
2. **Asynchronous Tasks:** Automatically changing PC status to `AVAILABLE` and notifying the frontend when a user's paid time expires.
3. **Role-Based Access Control (RBAC):** Strict endpoint protection to ensure Users cannot access Admin/Owner financial data.