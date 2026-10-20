# Marketplace електроніки

Вебзастосунок маркетплейсу електроніки для розміщення та перегляду оголошень.

## Технології

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Git

## Архітектура

Проєкт побудований за багатошаровою архітектурою:

Controller → Service → Repository → Database

### Проєкти

- `Marketplace.Api` — HTTP API та контролери.
- `Marketplace.Application` — бізнес-логіка, сервіси та інтерфейси Repository.
- `Marketplace.Domain` — сутності предметної області.
- `Marketplace.Infrastructure` — робота з базою даних та реалізація Repository.

## Основні сутності

- User
- Category
- Listing
- Favorite
- Message

## База даних

Для роботи з базою даних використовується SQLite.

Файл бази даних:

`marketplace.db`

Структура бази створюється за допомогою Entity Framework Core Migrations.

Перша міграція:

`InitialCreate`

## Запуск

1. Відкрити рішення `Marketplace.Api.sln`.
2. Запустити проєкт `Marketplace.Api`.
3. Перевірити health endpoint:

`GET /health`

Очікувана відповідь:

```json
{
  "status": "healthy"
}