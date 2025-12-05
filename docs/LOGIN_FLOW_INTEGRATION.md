# Login Flow Integration Guide

## Overview

Hướng dẫn chi tiết để tích hợp luồng login vào Frontend. Luồng bao gồm:
1. **Email Verification** - Nếu email chưa xác nhận, OTP sẽ được gửi **trong quá trình login**
2. **Two-Factor Authentication (2FA)** - Verify 2FA nếu user bật
3. **Token Generation** - Nhận access token và refresh token

---

## 0. Pre-requisite: User Registration (Signup)

Trước khi có thể đăng nhập, người dùng phải đăng ký tài khoản và xác nhận email của họ.

**Backend Endpoints:**
- `POST /api/users` - Tạo tài khoản người dùng mới
- `POST /api/auth/verify-email-otp` - Xác nhận email bằng OTP

**Flow:**
1. User nhập thông tin (email, password, v.v.)
2. Backend gửi OTP đến email người dùng
3. User xác nhận email bằng OTP
4. Sau khi email được xác nhận, user có thể đăng nhập

**Note:** Hướng dẫn này tập trung vào **Login Flow**. Chi tiết Signup/Registration flow nằm trong tài liệu riêng.

---

## 1. Login Step 1: Credentials & Email Verification

### 1.1 User nhập username/email và password

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "identity": "vuvananh010203@gmail.com",
  "password": "Admin@123",
  "rememberMe": false
}
```

### 1.2 Possible Responses

#### A. Email Not Verified (200) - OTP Sent

Khi email chưa verify, backend sẽ tự động gửi OTP và trả về response này:

```json
{
  "requiresEmailVerification": true,
  "emailVerificationSessionToken": "ZW1haWxAZXhhbXBsZS5jb206MTo...",
  "maskedEmail": "e***l@example.com",
  "user": null,
  "accessToken": "",
  "refreshToken": "",
  "expiresIn": 0,
  "requiresTwoFactor": false
}
```

**UI Action:**
- Display: "Mã xác nhận đã được gửi đến {maskedEmail}"
- Show OTP input screen (6 digits)
- Save: `emailVerificationSessionToken` (use for next step)
- Show: `maskedEmail` to user
- Go to: **Step 1.3** (Verify Email OTP)

#### B. Account Locked (401)
```json
{
  "type": "https://api.fam.com/errors/unauthorized",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Account is locked. Try again in X minutes.",
  "instance": "/api/auth/login",
  "errors": [
    {
      "code": "AUTH_ACCOUNT_LOCKED",
      "detail": "Account is locked. Try again in X minutes.",
      "details": {
        "minutesRemaining": 14,
        "lockoutEnd": "2025-12-05T14:00:00Z"
      }
    }
  ]
}
```

**UI Action:**
- Display error message từ `detail` field
- Show countdown timer từ `minutesRemaining`
- Disable login button

#### C. Invalid Credentials (401)
```json
{
  "type": "https://api.fam.com/errors/unauthorized",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid username/email or password",
  "instance": "/api/auth/login",
  "errors": [
    {
      "code": "AUTH_INVALID_CREDENTIALS",
      "detail": "Invalid username/email or password"
    }
  ]
}
```

**UI Action:**
- Display: "Sai tên đăng nhập hoặc mật khẩu"
- Clear password field
- Focus on identity field

#### D. Account Inactive (401)
```json
{
  "type": "https://api.fam.com/errors/unauthorized",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Account is inactive",
  "instance": "/api/auth/login",
  "errors": [
    {
      "code": "AUTH_ACCOUNT_INACTIVE",
      "detail": "Account is inactive"
    }
  ]
}
```

**UI Action:**
- Display: "Tài khoản không hoạt động"
- Contact support

#### E. Success - 2FA Required (200)
```json
{
  "requiresTwoFactor": true,
  "twoFactorSessionToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@fam.local",
    "firstName": "System",
    "lastName": "Administrator",
    "fullName": "System Administrator",
    "avatar": "https://gravatar.com/...",
    "phoneNumber": "0123456789",
    "phoneCountryCode": "+84",
    "dateOfBirth": "2003-01-01T00:00:00Z",
    "bio": null,
    "isEmailVerified": true,
    "isTwoFactorEnabled": true,
    "preferredLanguage": "en",
    "timeZone": "UTC"
  }
}
```

**UI Action:**
- Save: `twoFactorSessionToken` (use for verify 2FA)
- Display user info
- Go to: **Step 2** (Verify 2FA)

#### F. Success - Direct Login (200)
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@fam.local",
    "firstName": "System",
    "lastName": "Administrator",
    "fullName": "System Administrator",
    "avatar": "https://gravatar.com/...",
    "phoneNumber": "0123456789",
    "phoneCountryCode": "+84",
    "dateOfBirth": "2003-01-01T00:00:00Z",
    "bio": null,
    "isEmailVerified": true,
    "isTwoFactorEnabled": false,
    "preferredLanguage": "en",
    "timeZone": "UTC"
  }
}
```

**UI Action:**
- Save: `accessToken`, `refreshToken`, `user`
- Redirect to: Dashboard/Home
- Store user info in local state/context

---

### 1.3 Verify Email OTP (When Required)

Chỉ thực hiện nếu login response có `requiresEmailVerification: true`

**Request:**
```http
POST /api/auth/verify-email-otp
Content-Type: application/json

{
  "emailOtp": "123456"
}
```

#### A. Invalid/Expired OTP (401)
```json
{
  "type": "https://api.fam.com/errors/unauthorized",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid or expired OTP code",
  "errors": [
    {
      "code": "AUTH_INVALID_2FA_CODE",
      "detail": "Invalid or expired OTP code"
    }
  ]
}
```

**UI Action:**
- Display: "Mã xác nhận không đúng hoặc đã hết hạn"
- Clear OTP input
- Show: "Gửi lại mã" button

#### B. Success (200) - Email Verified
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@fam.local",
    "firstName": "System",
    "lastName": "Administrator",
    "fullName": "System Administrator",
    "avatar": "https://gravatar.com/...",
    "phoneNumber": "0123456789",
    "phoneCountryCode": "+84",
    "dateOfBirth": "2003-01-01T00:00:00Z",
    "bio": null,
    "isEmailVerified": true,
    "isTwoFactorEnabled": false,
    "preferredLanguage": "en",
    "timeZone": "UTC"
  },
  "requiresTwoFactor": false
}
```

**UI Action:**
- Email verification complete ✅
- Save: `accessToken`, `refreshToken`, `user`
- Redirect to: Dashboard/Home
- Store user info in local state/context

---

## 2. Login Step 2: Two-Factor Authentication (2FA)

Chỉ hiển thị nếu `requiresTwoFactor: true` từ login response

### 2.1 Verify 2FA Code

**Request:**
```http
POST /api/auth/verify-2fa
Content-Type: application/json
Authorization: Bearer {twoFactorSessionToken}

{
  "twoFactorCode": "123456",
  "twoFactorSessionToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "rememberMe": false
}
```

#### A. Invalid 2FA Code (401)
```json
{
  "type": "https://api.fam.com/errors/unauthorized",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid 2FA code",
  "errors": [
    {
      "code": "AUTH_INVALID_2FA_CODE",
      "detail": "Invalid 2FA code"
    }
  ]
}
```

**UI Action:**
- Display: "Mã xác nhận không đúng"
- Clear OTP input
- Show retry button

#### B. Success (200)
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@fam.local",
    "firstName": "System",
    "lastName": "Administrator",
    "fullName": "System Administrator",
    "avatar": "https://gravatar.com/...",
    "phoneNumber": "0123456789",
    "phoneCountryCode": "+84",
    "dateOfBirth": "2003-01-01T00:00:00Z",
    "bio": null,
    "isEmailVerified": true,
    "isTwoFactorEnabled": true,
    "preferredLanguage": "en",
    "timeZone": "UTC"
  }
}
```

**UI Action:**
- Save: `accessToken`, `refreshToken`, `user`
- Redirect to: Dashboard/Home
- Store user info in local state/context

---

## 3. Complete Login Flow Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                          LOGIN FLOW                                  │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                    User enters credentials
                  (username/email + password)
                           │
                           ▼
            ┌──────────────────────────────────┐
            │   POST /api/auth/login           │
            │   {identity, password, ...}      │
            └──────────────┬───────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
        ▼                  ▼                  ▼
    401 INVALID        200 EMAIL          200 SUCCESS
    CREDENTIALS        NOT VERIFIED
        │                  │                  │
        │            ┌─────┴──────┐        ┌──┴──────┐
        │            │            │        │         │
        ▼            ▼            ▼        ▼         ▼
    Show Error   Send OTP    2FA        NO 2FA
    Message      Auto        Required
    "Sai pass"
                 ┌────────────────────────┐
                 │ POST /api/auth/        │
                 │ verify-email-otp       │
                 │ {emailOtp}             │
                 └──────────┬─────────────┘
                            │
                 ┌──────────┴──────────┐
                 │                     │
                 ▼                     ▼
             401 INVALID          200 SUCCESS
             OTP                  OTP Valid
                 │                    │
                 ▼                    │
             Show Error              │
             "Sai mã OTP"            │
                                     ▼
                    ┌────────────────────────────┐
                    │ Check 2FA Status          │
                    └──────────┬─────────────────┘
                               │
                   ┌───────────┴───────────┐
                   │                       │
                   ▼                       ▼
                2FA REQUIRED            NO 2FA
                   │                       │
                   ▼                       ▼
        POST /api/auth/      Return Access Token
        verify-2fa              & Refresh Token
        {code}
           │
        ┌──┴──┐
        │     │
        ▼     ▼
      401   200
    INVALID SUCCESS
      │       │
      ▼       ▼
    Error  🎉 LOGIN SUCCESS
          Redirect to Dashboard
```

---

## 4. Error Code Reference

| Error Code | HTTP Status | Meaning | UI Action |
|---|---|---|---|
| `AUTH_INVALID_CREDENTIALS` | 401 | Sai email hoặc password | Clear form, show error |
| `AUTH_EMAIL_NOT_VERIFIED` | 401 | Email chưa xác nhận (phải xác nhận lúc signup) | Contact admin to verify email |
| `AUTH_ACCOUNT_LOCKED` | 401 | Tài khoản bị khóa (5 lần sai password) | Show countdown timer |
| `AUTH_ACCOUNT_INACTIVE` | 401 | Tài khoản không hoạt động | Contact support |
| `AUTH_INVALID_2FA_CODE` | 401 | Sai mã 2FA | Clear OTP, retry |

---

## 5. State Management Example

```typescript
interface LoginState {
  // Step 1: Email Verification
  email?: string;
  password?: string;
  needsEmailVerification?: boolean;
  emailOtpSent?: boolean;
  
  // Step 2: 2FA
  requiresTwoFactor?: boolean;
  twoFactorSessionToken?: string;
  
  // Step 3: Success
  accessToken?: string;
  refreshToken?: string;
  user?: UserInfo;
  
  // UI State
  isLoading?: boolean;
  error?: {
    code: string;
    message: string;
    details?: Record<string, any>;
  };
}

interface UserInfo {
  id: number;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  avatar?: string;
  phoneNumber?: string;
  phoneCountryCode?: string;
  dateOfBirth?: string;
  bio?: string;
  isEmailVerified: boolean;
  isTwoFactorEnabled: boolean;
  preferredLanguage?: string;
  timeZone?: string;
}
```

---

## 6. Frontend Implementation Checklist

- [ ] **Login Form**
  - [ ] Input: Email/Username
  - [ ] Input: Password
  - [ ] Checkbox: Remember me
  - [ ] Button: Login
  - [ ] Error handling for: AUTH_EMAIL_NOT_VERIFIED, AUTH_INVALID_CREDENTIALS, AUTH_ACCOUNT_LOCKED, AUTH_ACCOUNT_INACTIVE

- [ ] **2FA Verification Screen** (if needed)
  - [ ] Display: "Enter code from Authenticator app"
  - [ ] Input: 6-digit OTP
  - [ ] Button: Verify 2FA
  - [ ] Button: Use recovery code (if supported)
  - [ ] Countdown timer for code validity
  - [ ] Error handling for: AUTH_INVALID_2FA_CODE

- [ ] **Success Screen**
  - [ ] Display: User profile information (firstName, lastName, avatar, etc.)
  - [ ] Redirect to: Dashboard/Home after successful login

- [ ] **Error Handling**
  - [ ] Map error codes to localized messages
  - [ ] Display detailed error messages from API
  - [ ] Handle network errors gracefully
  - [ ] Retry logic for failed requests

- [ ] **Security**
  - [ ] Store tokens in secure storage (httpOnly cookies or encrypted localStorage)
  - [ ] Add `Authorization: Bearer {accessToken}` to all API requests
  - [ ] Implement token refresh before expiry
  - [ ] Logout on token expiry
  - [ ] Clear sensitive data on logout

- [ ] **UX Improvements**
  - [ ] Show loading spinner during requests
  - [ ] Disable buttons during loading
  - [ ] Auto-focus on input fields
  - [ ] Toast notifications for errors/success
  - [ ] Remember device option with 2FA
  - [ ] Show user info immediately after successful login

---

## 7. API Endpoints Summary

| Endpoint | Method | Purpose | Status |
|---|---|---|---|
| `/api/auth/login` | POST | Login with credentials | ✅ Implemented |
| `/api/auth/verify-2fa` | POST | Verify 2FA code and complete login | ✅ Implemented |
| `/api/auth/refresh` | POST | Refresh access token | ✅ Implemented |
| `/api/auth/logout` | POST | Logout user | ✅ Implemented |
| `/api/auth/logout-all` | POST | Logout from all devices | ✅ Implemented |
| `/api/auth/change-password` | POST | Change password | ✅ Implemented |
| `/api/auth/forgot-password` | POST | Request password reset | ✅ Implemented |
| `/api/auth/verify-reset-token` | POST | Verify reset token validity | ✅ Implemented |
| `/api/auth/reset-password` | POST | Reset password with token | ✅ Implemented |
| `/api/auth/verify-email-otp` | POST | Verify email with OTP | ✅ Implemented |

**Note**: Email verification is performed during user registration/signup flow, not during login. Users must have a verified email address to log in. If email is not verified, the login endpoint returns `AUTH_EMAIL_NOT_VERIFIED` error.

---

## 8. Environment Variables (Frontend)

```env
VITE_API_BASE_URL=http://localhost:8000
VITE_API_TIMEOUT=30000
```

---

## 9. Important Notes

### Token Management
- **Access Token**: Expires in 1 hour (3600 seconds)
- **Refresh Token**: Expires in 7-30 days based on `rememberMe` flag
  - `rememberMe: false` → 7 days
  - `rememberMe: true` → 30 days
- Store tokens securely (httpOnly cookies or encrypted localStorage)

### 2FA Session Token
- Temporary token for 2FA verification
- Valid for a limited time (~5-10 minutes)
- Only used to complete the login after credentials are verified

### Password Reset Flow
- Use `POST /api/auth/forgot-password` to request password reset
- Backend sends reset link via email
- Link includes token that expires in 15 minutes (configurable via appsettings)
- Use `POST /api/auth/verify-reset-token` to check token validity
- Use `POST /api/auth/reset-password` to set new password

### Account Lockout
- **Threshold**: 5 failed login attempts
- **Lockout Duration**: 15 minutes
- After lockout, error code `AUTH_ACCOUNT_LOCKED` is returned with `minutesRemaining` detail

### Timestamps
- All timestamps are in UTC ISO 8601 format
- Example: `2024-12-05T14:00:00Z`

### CORS & Rate Limiting
- Rate limit on `/api/auth/login` and `/api/auth/verify-2fa`: 5 requests per minute per IP
- CORS configured for frontend domain
- Include credentials in fetch requests if using cookies for token storage

---

## 10. Troubleshooting

| Issue | Cause | Solution |
|---|---|---|
| `AUTH_EMAIL_NOT_VERIFIED` when registering | Email verification not completed during signup | Complete email verification in signup flow first |
| `AUTH_INVALID_CREDENTIALS` | Wrong password or username | Check username/email and password |
| `AUTH_ACCOUNT_LOCKED` | Too many failed login attempts | Wait 15 minutes and try again |
| Token always expires | Token refresh logic not implemented | Implement token refresh before expiry or after 401 response |
| 2FA always required | 2FA enabled for user | User can disable 2FA in settings |
| CORS error | Origin not whitelisted | Ensure frontend URL is in CORS configuration |

---

## 11. Related Endpoints (Not covered in Login Flow)

| Endpoint | Purpose |
|---|---|
| `POST /api/users` | Create new user (Signup) |
| `POST /api/auth/change-password` | Change password after login |
| `POST /api/auth/enable-2fa` | Enable 2FA |
| `POST /api/auth/disable-2fa` | Disable 2FA |
| `POST /api/auth/confirm-2fa` | Confirm 2FA setup during enable |
