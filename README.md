<h1 align="center" id='top'>
  CyberZone v0.0.1
  <br>
</h1>

<h4 align="center">Comprehensive Cyber Cafe Management System</h4>

## Table of contents

<ul>
  <li>
    <a href="#about">About</a>
  </li>
  <li>
    <a href="#key-features">Key Features</a>
  </li>
  <li>
    <a href="#how-to-use">How to use</a>
  </li>
  <li>
    <a href="#documentation">Documentation</a>
  </li>
  <li>
    <a href="#feedback">Feedback</a>
  </li>
  <li>
    <a href="#license">License</a>
  </li>
  <li>
    <a href="#contacts">Contacts</a>
  </li>
</ul>

<div id='about'>

## About 
Managing a modern cyber cafe can be chaotic. Between tracking PC reservations, managing billing, monitoring hardware status, and selling bar items, administrators often find themselves overwhelmed by fragmented systems. 

Our mission is to create a comprehensive web application that automates and streamlines the operations of a modern cyber cafe. We believe that Gamers deserve a seamless self-service experience, while Cafe Owners and Shift Admins need a powerful, unified dashboard to control their business.

***Why CyberZone?***

While analyzing cyber cafe operations, we noticed the pain of:

- Time wasted manually tracking user sessions and resolving hardware conflicts
- Lack of self-service booking for gamers, leading to queues at the admin desk
- Disconnected bar inventory and gaming billing systems
- Difficulty for owners to track real-time revenue, PC utilization, and platform analytics

***CyberZone*** is the result of applying modern technical solutions (State Machines, Async Tasks, and strict Role-Based Access Control) to create a flawless experience for Guests, Users, Cafe Admins, and Platform Super-Admins.

</div>

<div id='key-features'>

## Key Features

- **Interactive Seating & Booking:** Real-time visual map of PC/Console statuses (Available, Occupied, Booked, Maintenance). Gamers can easily find and reserve seats in specific zones without admin intervention.
- **Dynamic Time-Billing & Finance:** Flexible pricing engine for Cafe Owners. Configure hourly rates based on zones (e.g., Standard vs. VIP RTX 4090) or time slots (e.g., Fixed-price "Night Mode"). Users can easily top up their virtual balance.
- **Advanced Session Control:** Gamers can start, pause, or terminate their sessions. The system automatically tracks active time, manages PC states via a robust State Machine, and uses Asynchronous Tasks to free up PCs when paid time expires.
- **Integrated Food & Beverage:** A built-in bar menu where Gamers can order snacks and drinks directly to their PC. The system auto-detects the PC number, deducts items from inventory, and updates the user's final bill.
- **Role-Based Access Control (RBAC):** - *Guests/Users:* Catalog browsing, booking, session management, and profile settings.
  - *Cafe Admins:* Local hardware management, session overrides, bar inventory, and local analytics.
  - *Platform Admins:* Global moderation, conflict resolution, platform-wide analytics, and club registration approvals.
- **Social & Analytics Dashboards:** Post-session rating system for gamers, coupled with deep financial and utilization dashboards for Owners to maximize efficiency.

</div>

<div id='how-to-use'>
  
## How to use

</div>

<div id='documentation'>

## Documentation



The CyberZone platform is divided into several core modules. Below is the detailed documentation of system capabilities based on User Roles (`Guest`, `User`, `Club Admin`, `System Admin`).

### 1. Identity & Access Management
Handles authentication, authorization, and account setups.
* **Guest:** Can register a new account, log in, and recover a lost password.
* **User:** Can edit profile details, change passwords for security, and link external gaming accounts (e.g., Steam, Riot Games).
* **Club Admin:** Can apply to register a new club on the platform and log into their club's dashboard.
* **System Admin:** Logs into the super-admin panel, handles user bans/unbans, recovers Club Admin passwords, and approves/rejects new club registration requests.

### 2. Catalog & Venue Profile
Manages the visibility and details of partner cyber cafes.
* **Guest / User:** Can browse the catalog of all partner clubs and view specific venue details (location, hardware, pricing).
* **Club Admin:** Can update their club's profile, including photos, working hours, hardware specs, and pricing information.

### 3. Booking & Scheduling
The core reservation engine for PCs and Consoles.
* **Guest / User:** Can view real-time availability of seats, create reservations, modify booking times, or cancel them entirely.
* **Club Admin:** Has access to a comprehensive grid of past, current, and future reservations. Can manually assign a user to a seat to override or update the system state.

### 4. Session & Hardware Management
Controls the actual gaming time and physical equipment states.
* **User:** Can independently start, pause (for valid reasons like breaks), and end their gaming session.
* **Club Admin:** Manages the floor plan by adding, removing, or editing seat details. Can change a seat's status to "Under Maintenance", preventing any bookings for that specific PC/Console.

### 5. Billing & Finance
Handles all monetary transactions and pricing models.
* **User:** Can top up their internal platform balance and view a detailed history of their transactions.
* **Club Admin:** Can create and modify pricing plans (e.g., standard hourly rate, night packages).
* **System Admin:** Sets global platform commissions for specific services and configures overall payment processing gateways.

### 6. Food & Beverage (Bar integration)
Seamless in-seat ordering system.
* **User:** Can browse the available bar menu, check stock, and form a cart. Orders are sent directly to their active PC. Payments can be deducted immediately from the virtual balance or added to the final session bill.
* **Club Admin:** Manages the menu catalog (add/edit items), controls inventory levels to prevent out-of-stock orders, and receives real-time notifications with the order details and target PC number.

### 7. Social & Moderation
Maintains platform quality and resolves disputes.
* **User:** Prompted to leave a 5-star rating and text review *after* completing a gaming session.
* **System Admin:** Acts as a moderator to delete inappropriate comments and step in to resolve conflicts between a User and a Club Admin.

### 8. Analytics & Reporting
Data-driven insights for venue owners and platform runners.
* **Club Admin:** Views local metrics: daily/monthly revenue, and PC utilization rates (percentage of time machines are occupied).
* **System Admin:** Views global metrics: total platform revenue across all clubs, active user counts, and system load/server health.

</div>

<div id='feedback'>

## Feedback
  
</div>

<div id='license'>

## License
MIT
  
</div>

<div id='contacts'>

## Contacts
  
For more details about our product or any general information regarding CyberZone, feel free to reach out to us.

We are here to provide support and answers any questions you may have. Below are the way to contact our team:

**Email**: Send us your inquiries or support request at cyberzone@gmail.com

Subscribe to our`s **LinkedIn** profiles:
- <a href="https://www.linkedin.com/in/kseniia-mekheda-a83032382/"> Kseniia Mekheda</a>
- <a href="https://www.linkedin.com/in/markiyan-bevz-7a2677285/"> Markiyan Bevz</a>
- <a href="https://www.linkedin.com/in/віталій-доманов-2a1460357/"> Vitalii Domanov</a>

We look forward to assisting you and ensuring your experience with our products is successful and enjoyable!

<a href="#top">Back to top</a>
</div>
