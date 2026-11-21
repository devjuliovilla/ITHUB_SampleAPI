# ITHUB_SampleAPI — Postman

Postman collection for the ITHUB_SampleAPI demo API. Import this file and `ITHUB_SampleAPI.postman_environment.json`, select the local environment, then fill in the editable account variables. Run **Login** and **Login as administrator** first; the collection stores their tokens separately. Task, activity, status, and assigned-user IDs are editable environment values.

## Endpoints

| Method | Endpoint | Function |
| --- | --- | --- |
| `POST` | `/api/auth/login` | Authenticate and return a JWT. |
| `POST` | `/api/users` | Admin-only creation of a `User` account. |
| `GET` | `/api/users/me` | Return the authenticated user's profile. |
| `GET` | `/api/tasks` | List all tasks for Admin or assigned tasks for User. |
| `GET` | `/api/tasks/{taskId}` | Get a task visible to the caller. |
| `POST` | `/api/tasks` | Admin creates and assigns a task to an active user. |
| `POST` | `/api/tasks/{taskId}/activities` | Assigned user or Admin adds an activity. |
| `PATCH` | `/api/tasks/{taskId}/activities/{activityId}/status` | Activity author or Admin changes its `statusId`. |
| `GET` | `/api/statuses?scope=Task\|Activity` | List active statuses for the requested scope. |
| `GET` | `/api/health/live` | Public process liveness check. |
| `GET` | `/api/health/ready` | Public readiness and database connectivity check. |

## Source repository

https://github.com/devjuliovilla/ITHUB_SampleAPI
