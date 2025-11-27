# ⚡ EV-Battery-Trading-Platform
Second-hand EV & Battery Trading Platform
- A modern backend system for buying, selling, and auctioning used electric vehicles (EV) and EV batteries.
- Built with C# .NET, Docker, RabbitMQ, Kong API Gateway, and microservice architecture.

# 🚀 Overview
The EV-Battery Trading Platform is a service marketplace allowing users to trade second-hand electric vehicles and EV batteries.
It provides listing management, auctions, buyer/seller interactions, digital contracts, and a complete admin control panel.

This system supports:
- Sustainable EV ecosystem
- User-to-user marketplace
- Safety & quality control through admin moderation
- Scalable microservices communication

# 🧱 Tech Stack
| Category          | Technology                                       |
| ----------------- | ------------------------------------------------ |
| Backend Framework | **C# .NET**                                      |
| Architecture      | **Microservices**                                |
| Communication     | **RabbitMQ (Event-driven)**                      |
| API Gateway       | **Kong Gateway**                                 |
| Database          | PostgreSQL / SQL Server (depending on your repo) |
| Authentication    | JWT Auth / Identity Service                      |
| Deployment        | **Docker / Docker Compose**                      |
| Monitoring        | (Optional) Grafana / Prometheus                  |
| Load Balancing    | Kong + Service Discovery                         |


# 👥 Actors
1. Guest
- Can browse listings
- Can search vehicles/batteries
- Cannot buy or sell until registered
  
2. Member (User)
- Registered users who can buy/sell and participate in trading.

3. Admin
- System controllers who review, approve, and manage platform activity.

# 👥 Actors
**1️⃣ Member Features**
**a. Account Registration & Management**

**Register/Login via**
- Email
- Phone number
- Social login (optional)

**Manage profile:**

**Personal information**

**AI-based price suggestion based on:**
- Market data
- Historical transactions
- Owned EV/battery
- Transaction history

**b. Post EV/Battery Listings**

**Create listings for:**

- Used EVs
- EV batteries
**Upload images**
**Provide specifications**
**AI-based price suggestion based on:**
- Market data
- Historical transactions

**c. Search & Purchase**

**Search vehicles/batteries by:**

- Brand, model
- Battery capacity
- Price range
- Condition
- Mileage
- Year of manufacture

**Save favorites**
  
**Compare listings**
  
**Buy instantly or join auctions**

**d. Transactions & Payments**

Online payment (e-wallet, banking, etc.)

Digital contract signing

Secure transaction flow

**e. After-Sale Support**

Rate sellers/buyers

Provide feedback

View full purchase history

**2️⃣ Admin Features**

**a. User Management**

Approve accounts

Ban/lock users

View user activity

**b. Listing Management**
Approve listings

Remove spam or invalid posts

Tag listings as “Verified”

**c. Transaction Administration**
Monitor transactions

Handle disputes & complaints

**d. Fee & Commission Management**
Set transaction fees

Adjust marketplace commission rate

**e. Reporting & Analytics**
Total transactions

Revenue dashboard

Market trends & insights

# 🐳 Docker Support
The entire project supports Docker Compose:
**Start the full system:**
```bash
docker-compose up -d
```

**Includes:**
- All microservices
- RabbitMQ
- Kong Gateway
- Databases

#🔀 RabbitMQ Event-Driven System
Used for:
- Routing
- Load balancing
- Rate limiting
- Authentication enforcement
- Request logging

# 🌐 Kong API Gateway
Kong handles:
- Routing
- Load balancing
- Rate limiting
- Authentication enforcement
- Request logging

All services are exposed through a unified gateway.

# 📌 Summary
**This platform provides:**

✔️ Second-hand EV & battery marketplace

✔️ Auctions + Direct purchase

✔️ Microservice architecture

✔️ Event-driven communication with RabbitMQ

✔️ API Gateway (Kong)

✔️ Admin moderation & analytics

✔️ Secure digital transactions


