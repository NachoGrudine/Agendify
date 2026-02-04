# 📅 Agendify

Appointment and scheduling management system for small service businesses (hair salons, barbershops, clinics).

## 🎯 What is Agendify?

Agendify is a tool that allows business owners to manage appointments, employees, and clients from a visual control panel. Forget about the notebook and centralize all operations in one place.

**"Canvas" Philosophy:** Total flexibility. Create quick appointments without rigid structure, as if you were writing on a sheet of paper.

## 🏗️ Architecture and Tech Stack

### Backend (.NET 10)
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Patterns:** Layered architecture (separation of concerns)
- **Main Libraries:**
  - FluentValidation (validations)
  - FluentResults (result handling)
  - FluentAssertions (testing)
  - Fluent API (EF configuration)
  - LINQ (queries)
  
### Frontend (Angular 21)
- **UI Framework:** PrimeNG
- **Icons:** Lucide Icons
- **Reactivity:** RxJS + Signals
- **Language:** TypeScript
- **Architecture:** Reusable generic components

### Testing
- **Framework:** xUnit
- **Mocking:** Moq

### DevOps
- **Orchestration:** Docker Compose (runs the entire application together)

## 🚀 Key Features

- **Multi-tenant:** Complete isolation between businesses
- **Provider Management:** Handle employees/resources with complex schedules
- **Flexible Scheduling:** Appointments without fixed time slots (continuous time)
- **Smart Validations:** Prevention of overlaps and schedule conflicts
- **Automatic Onboarding:** Upon registration, the system automatically creates your business and your first provider
