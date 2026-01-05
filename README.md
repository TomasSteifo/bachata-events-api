# bachata-events-api

# Use Cases — Bachata Festival Platform (MVP)

## UC-01 — Browse Festivals (Dancer / Public User)

**Primary actor:** Dancer (Public User)

**Description:**  
A dancer wants to browse upcoming bachata festivals and find events that match their availability and location.

**Preconditions:**
- User is not logged in
- At least one festival exists and is published

**Main flow:**
1. User opens the website
2. User navigates to **Festivals**
3. System displays a list of upcoming festivals
4. User applies filters (country, date range)
5. System updates the list based on selected filters
6. User clicks a festival to view details

**Postconditions:**
- User sees festival details


---

## UC-02 — View Festival Details

**Primary actor:** Dancer (Public User)

**Description:**  
A dancer wants to view full information about a selected festival.

**Main flow:**
1. User selects a festival from the festival list
2. System displays:
   - Festival name
   - Dates
   - Location
   - Description
   - Organizer information
   - External links (website, tickets, socials)


---

## UC-03 — Register as Organizer

**Primary actor:** Organizer

**Description:**  
A user wants to register as an organizer in order to publish festivals.

**Main flow:**
1. User opens the **Register** page
2. User selects role = **Organizer**
3. User submits the registration form
4. System creates:
   - User account
   - Organizer profile

**Postconditions:**
- User can log in as Organizer
- Organizer dashboard becomes available


---

## UC-04 — Organizer Creates Festival

**Primary actor:** Organizer

**Description:**  
An organizer wants to create and publish a new festival.

**Preconditions:**
- Organizer is logged in

**Main flow:**
1. Organizer opens **Dashboard**
2. Organizer clicks **Create Festival**
3. Organizer fills in festival form:
   - Name
   - Dates
   - Country / City
   - Description
   - External links
4. Organizer saves the festival
5. Organizer publishes the festival

**Postconditions:**
- Festival is visible to dancers and public users


---

## UC-05 — Organizer Manages Own Festivals

**Primary actor:** Organizer

**Description:**  
An organizer wants to edit or delete festivals they own.

**Preconditions:**
- Organizer is logged in
- Festival belongs to the organizer

**Main flow:**
1. Organizer opens **My Festivals**
2. Organizer selects a festival
3. Organizer edits or deletes the festival
4. System validates ownership

**Postconditions:**
- Festival is updated or removed from public listings


---

## UC-06 — Authorization & Ownership (VG)

**Primary actor:** System

**Description:**  
System enforces authorization and ownership rules.

**Rules:**
- Only users with role **Organizer** can create festivals
- Only the festival owner can edit or delete a festival
- Dancers (public users) have read-only access
- Unauthorized actions are rejected with proper error responses
